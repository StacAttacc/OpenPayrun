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
      in
      {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            dotnet-sdk_9
            nodejs_22
            git
            csharpier
          ];

          DOTNET_ROOT = "${pkgs.dotnet-sdk_9}";

          shellHook = ''
            export NUGET_PACKAGES=$PWD/.nuget/packages
            export NPM_CONFIG_PREFIX=$PWD/.npm-global
            export PATH=$NPM_CONFIG_PREFIX/bin:$PATH
            if ! command -v ng &>/dev/null; then
              echo "  installing Angular CLI..."
              npm install -g @angular/cli --silent
            fi

            echo ""
            echo "  dotnet : $(dotnet --version)"
            echo "  node   : $(node --version)"
            echo "  ng     : $(ng version --skip-confirmation 2>/dev/null | grep 'Angular CLI' | awk '{print $NF}' || echo n/a)"
            echo ""
          '';
        };
      });
}
