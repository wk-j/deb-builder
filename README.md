## Deb Builder

[![NuGet](https://img.shields.io/nuget/v/wk.DebBuilder.svg)](https://www.nuget.org/packages/wk.DebBuilder)

```bash
brew install dpkg
dotnet tool install -g wk.DebBuilder
```

## Usage

```bash
wk-deb-builder \
    --name         MyWeb \
    --version      2.0.0 \
    --exec-start  '/usr/bin/dotnet --urls="http://*:9999" /opt/MyWeb/MyWeb.dll' \
    --temp-dir    .temp \
    --app-dir     .publish \
    --output-dir  .output
```

## Development

```bash
rm -rf .temp
rm -rf .output
dotnet run --project src/DebBuilder/DebBuilder.fsproj -- \
    --name         MyWeb \
    --version      2.0.0 \
    --exec-start  "/usr/bin/dotnet /opt/MyWeb/MyWeb.dll" \
    --temp-dir    .temp   \
    --app-dir     .publish \
    --output-dir  .output
```