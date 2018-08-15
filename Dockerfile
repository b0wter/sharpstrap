FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out 

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
COPY scripts/entry_point /app/entry_point
RUN chmod +x /app/entry_point
RUN mkdir -p /mnt/yaml
ENTRYPOINT ["/app/entry_point"]
