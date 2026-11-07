### To start
```bash
# Setup DB
sudo -u postgres psql -c "CREATE USER tic_tac_toe_user WITH PASSWORD 'tic_tac_toe_password';"
sudo -u postgres psql -c "CREATE DATABASE tic_tac_toe_db OWNER tic_tac_toe_user;"

# Migrate
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update --project Datasource/Datasource.csproj --startup-project Web/Web.csproj

# Run
dotnet run --project Web/Web.csproj
```
