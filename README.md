## Deb Builder

## Development

```bash
rm -rf .temp
rm -rf .output
dotnet run --project src/DebBuilder/DebBuilder.fsproj -- \
    --name         MyWeb \
    --version      2.0.0 \
    --exec-start  "dotnet /opt/MyWeb/MyWeb.dll" \
    --temp-dir    .temp   \
    --app-dir     .publish \
    --output-dir  .output
```