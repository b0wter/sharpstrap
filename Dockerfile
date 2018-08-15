FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out 

# Build runtime image
FROM library/fedora:28

RUN rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
RUN dnf update -y
RUN dnf install -y dotnet-sdk-2.1.202 libstdc++ libunwind icu libicu libicu-devel sudo

WORKDIR /app
COPY --from=build-env /app/out .
COPY scripts/entry_point /app/entry_point
RUN chmod +x /app/entry_point && \
    mkdir -p /mnt/yaml

RUN useradd -ms /bin/bash dotnetuser
RUN usermod -aG wheel dotnetuser
RUN echo "dotnetuser:dotnet"|chpasswd

USER dotnetuser
ENTRYPOINT ["/app/entry_point"]
