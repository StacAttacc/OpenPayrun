{
  description = "Angular frontend + .NET backend";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
        mssqlPassword = "DevPassword123!";
      in
      {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            dotnet-sdk_9
            nodejs_22
            podman
            git
          ];

          DOTNET_ROOT = "${pkgs.dotnet-sdk_9}";
          NUGET_PACKAGES = "${toString ./.}/.nuget/packages";

          shellHook = ''
            export NPM_CONFIG_PREFIX=$PWD/.npm-global
            export PATH=$NPM_CONFIG_PREFIX/bin:$PATH
            if ! command -v ng &>/dev/null; then
              echo "  installing Angular CLI..."
              npm install -g @angular/cli --silent
            fi

            MSSQL_PASSWORD="${mssqlPassword}"

            if podman ps -a --filter "name=osbooks-mssql" -q 2>/dev/null | grep -q .; then
              if ! podman ps --filter "name=osbooks-mssql" -q 2>/dev/null | grep -q .; then
                podman start osbooks-mssql >/dev/null
              fi
            else
              podman run -d \
                --name osbooks-mssql \
                -e ACCEPT_EULA=Y \
                -e MSSQL_SA_PASSWORD="$MSSQL_PASSWORD" \
                -p 1433:1433 \
                -v osbooks-mssql-data:/var/opt/mssql \
                mcr.microsoft.com/mssql/server:2022-latest \
                >/dev/null
            fi

            echo -n "  waiting for SQL Server"
            until podman exec osbooks-mssql \
              /opt/mssql-tools18/bin/sqlcmd -S localhost \
              -U sa -P "$MSSQL_PASSWORD" -C \
              -Q "SELECT 1" >/dev/null 2>&1; do
              echo -n "."
              sleep 1
            done
            echo " ready"

            echo ""
            echo "  dotnet : $(dotnet --version)"
            echo "  node   : $(node --version)"
            echo "  ng     : $(ng version --skip-confirmation 2>/dev/null | grep 'Angular CLI' | awk '{print $NF}' || echo n/a)"
            echo "  mssql  : localhost:1433  sa / $MSSQL_PASSWORD"
            echo ""
          '';
        };
      });
}
