open System
open System.Reflection
open System.IO
open System.IO.Compression

type Service = {
    ExecStart: string
    WorkingDirectory: string
    Name: string
}

type Control = {
    Name: string
    Version: string
}

type Commands = {
    Name: string
    Version: string
    ExecStart: string
    AppDir: string
    TempDir: string
    OutputDir: string
}

let rec parseCommand argv command =
    match argv with
    | "--name" :: xs ->
        match xs with
        | name :: xss ->
            parseCommand xss { command with Commands.Name = name }
        | _ -> command
    | "--version" :: xs ->
        match xs with
        | version :: xss ->
            parseCommand xss { command with Version = version }
        | _ -> command
    | "--exec-start" :: xs ->
        match xs with
        | execStart :: xss ->
            parseCommand xss { command with ExecStart = execStart }
        | _ -> command
    | "--temp-dir" :: xs ->
        match xs with
        | tempDir :: xss ->
            parseCommand xss { command with TempDir = tempDir }
        | _ -> command
    | "--app-dir" :: xs ->
        match xs with
        | appPath :: xss ->
            parseCommand xss { command with AppDir =  appPath }
        | _ -> command
    | "--output-dir" :: xs ->
        match xs with
        | outputDir :: xss ->
            parseCommand xss { command with OutputDir = outputDir }
        | _ -> command
    | _ -> command

let getResource (asm: Assembly) path =
    use control = new StreamReader(asm.GetManifestResourceStream(path))
    control.ReadToEnd();

let writeService (service: Service) (template:string) (root: string) =
    let target = Path.Combine(root, "etc", "systemd", "system")
    if Directory.Exists target |> not then
        Directory.CreateDirectory target |> ignore

    let serviceFile = Path.Combine(target, sprintf "%s.service" service.Name)
    let data =
        template
            .Replace("{name}", service.Name)
            .Replace("{execStart}", service.ExecStart)
            .Replace("{workingDirectory}", service.WorkingDirectory)

    File.WriteAllText(serviceFile, data)

let writeControl (control: Control) (template: string) (root: string) =
    let target = Path.Combine(root, "DEBIAN")
    if Directory.Exists target |> not then
        Directory.CreateDirectory target |> ignore

    let controlFile = Path.Combine(target, "control")
    let data =
        template
            .Replace("{name}", control.Name)
            .Replace("{version}", control.Version)
    File.WriteAllText(controlFile, data)

let rec copyDirs (source: DirectoryInfo) (target: DirectoryInfo) =
    if target.Exists |> not then target.Create() |> ignore

    for  dir in source.GetDirectories() do
        copyDirs dir (target.CreateSubdirectory(dir.Name))
    for  file in source.GetFiles() do
        file.CopyTo(Path.Combine(target.FullName, file.Name)) |> ignore

let createZip (command: Commands) =
    let dir = command.OutputDir
    if Directory.Exists dir |> not then
        Directory.CreateDirectory dir |> ignore

    let outputName = sprintf "%s.%s.deb" command.Name command.Version
    let outputPath = Path.Combine(dir, outputName)

    ZipFile.CreateFromDirectory(command.TempDir, outputPath);

[<EntryPoint>]
let main argv =
    let asm = Assembly.GetCallingAssembly();
    let asmName = asm.GetName().Name;

    let control = sprintf "%s.root.DEBIAN.control" asmName
    let service = sprintf "%s.root.etc.systemd.system.{name}.service" asmName

    let controlText = getResource asm control
    let serviceText = getResource asm service

    let command =
        parseCommand
            (argv |> List.ofArray)
            { Name = "MyApp"
              Version = "0.1.0"
              ExecStart = "dotnet /opt/Myapp/MyApp.dll"
              AppDir = ".publish"
              OutputDir = ".output"
              TempDir = ".temp" }

    let sv =
        { Name = command.Name
          ExecStart = command.ExecStart
          WorkingDirectory = sprintf "/opt/%s" command.Name }

    let ctl =
        { Name = command.Name
          Version = command.Version }

    let tempDir = command.TempDir
    let appDir = command.AppDir

    writeService sv  serviceText  tempDir
    writeControl ctl controlText  tempDir

    let appDir = DirectoryInfo appDir
    let targetDir = DirectoryInfo (sprintf "%s/opt/%s" tempDir command.Name)

    copyDirs appDir targetDir
    createZip command
    0