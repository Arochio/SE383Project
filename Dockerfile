FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY Backend/Backend.csproj Backend/
RUN dotnet restore Backend/Backend.csproj
COPY Backend/ Backend/
RUN dotnet publish Backend/Backend.csproj --configuration Release --output /app/backend --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS backend
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=backend-build /app/backend .
ENTRYPOINT ["dotnet", "Backend.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend/Frontend.csproj Frontend/
RUN dotnet restore Frontend/Frontend.csproj
COPY Frontend/ Frontend/
RUN dotnet publish Frontend/Frontend.csproj --configuration Release --output /app/frontend --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS frontend
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=frontend-build /app/frontend .
ENTRYPOINT ["dotnet", "Frontend.dll"]

FROM mcr.microsoft.com/mssql/server:2022-latest AS database
USER root
ENV ACCEPT_EULA=Y \
    MSSQL_PID=Developer
COPY Database/init.sql /docker-entrypoint-initdb.d/init.sql
COPY Database/entrypoint.sh /usr/local/bin/database-entrypoint.sh
RUN chmod +x /usr/local/bin/database-entrypoint.sh
EXPOSE 1433
ENTRYPOINT ["/usr/local/bin/database-entrypoint.sh"]
