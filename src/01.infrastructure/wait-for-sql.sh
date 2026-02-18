#!/bin/sh
set -e

echo "Aguardando SQL Server estar pronto..."

until /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; do
  echo "SQL ainda não está pronto - aguardando 5 segundos..."
  sleep 5
done

echo "SQL Server pronto! Iniciando Service Bus Emulator..."
exec "$@"