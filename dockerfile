# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source


# copy csproj different layers

#this sln file need only for the docker


COPY *.csproj .

#restore
RUN dotnet restore

COPY . .



# copy everything else and build app

RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./

#copy db for presentetion
COPY --from=build /source/*.db ./
COPY --from=build /source/*.db-shm ./
COPY --from=build /source/*.db-wal ./
COPY --from=build /source/PlaceImages ./PlaceImages

ENTRYPOINT ["dotnet", "MAP.dll"]