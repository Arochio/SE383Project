FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

ENV ACCEPT_EULA=Y \
    MSSQL_PID=Developer \
    MSSQL_SA_PASSWORD=YourStrong!Passw0rd

COPY init.sql /docker-entrypoint-initdb.d/init.sql
COPY entrypoint.sh /usr/local/bin/database-entrypoint.sh

RUN chmod +x /usr/local/bin/database-entrypoint.sh

EXPOSE 1433

ENTRYPOINT ["/usr/local/bin/database-entrypoint.sh"]
