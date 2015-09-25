#r @"../packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO

let rootDir =  FullName (__SOURCE_DIRECTORY__ </> "..")
let configuration = "Release"
let outDir = rootDir </> "out"
let buildPackagesDir = rootDir </> "packages" </> "build"
let appExe = rootDir </> "src" </> "NLogResharperAnnotations" </> "bin" </> configuration </> "NLogResharperAnnotations.exe"

let project = "NLogResharperAnnotations"
let solutionFile  = rootDir </> project + ".sln"
let sourceProjects = rootDir </> "src/**/*.??proj"

/// The profile where the project is posted
let gitOwner = "vbfox" 
let gitHome = "https://github.com/" + gitOwner

/// The name of the project on GitHub
let gitName = "NLogResharperAnnotations"

/// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" ("https://raw.github.com/" + gitOwner)

// --------------------------------------------------------------------------------------
// Build steps
// --------------------------------------------------------------------------------------

// Parameter helpers to be able to get parameters from either command line or environment
let getParamOrDefault name value = environVarOrDefault name <| getBuildParamOrDefault name value

let getParam name = 
    let str = getParamOrDefault name ""
    match str with
        | "" -> None
        | _ -> Some(str)

// Read additional information from the release notes document
let release = LoadReleaseNotes (rootDir </> "Release Notes.md")

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|) (projFileName:string) = 
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" <| fun _ ->
    CleanDir outDir

    !! solutionFile
    |> MSBuild "" "Clean" [ "Configuration", configuration ]
    |> ignore

// --------------------------------------------------------------------------------------
// Build executable

Target "Build" <| fun _ ->
    !! solutionFile
    |> MSBuild "" "Rebuild" [ "Configuration", configuration ]
    |> ignore

// --------------------------------------------------------------------------------------
// Generate annotations

Target "Generate" <| fun _ ->
    let args = sprintf "-d \"%s\" -v \"%s\"" outDir release.NugetVersion
    let conf (startInfo:Diagnostics.ProcessStartInfo) = 
        startInfo.FileName <- appExe
        startInfo.Arguments <- args

    let result = ExecProcess conf (TimeSpan.FromMinutes(1.0))
    if result <> 0 then failwith "Generation failed"
    ()

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target "NuGet" <| fun _ ->
    !! (outDir </> "*.nuspec")
    |> Seq.iter (
        NuGetPackDirectly (fun p -> 
            { p with
                OutputPath = outDir
                Version = release.NugetVersion
                ReleaseNotes = toLines release.Notes
                WorkingDir = outDir
                ToolPath = buildPackagesDir </> "NuGet.CommandLine" </> "tools" </> "NuGet.exe"
            })
        )

Target "PublishNuget" <| fun _ ->
    let key =
        match getParam "nuget-key" with
        | Some(key) -> key
        | None -> getUserPassword "NuGet key: "
        
    Paket.Push <| fun p ->  { p with WorkingDir = outDir; ApiKey = key; PublishUrl = "https://resharper-plugins.jetbrains.com" }

// --------------------------------------------------------------------------------------
// Release Scripts

#load "../paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"

Target "GitRelease" (fun _ ->
    let remote =
        Git.CommandHelper.getGitResult "" "remote -v"
        |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
        |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
        |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]
            
    Git.Staging.StageAll ""
    Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Git.Branches.pushBranch "" remote (Git.Information.getBranchName "")

    Git.Branches.tag "" release.NugetVersion
    Git.Branches.pushTag "" remote release.NugetVersion
)

Target "GitHubRelease" (fun _ ->
    let user =
        match getBuildParam "github-user" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserInput "GitHub Username: "
    let pw =
        match getBuildParam "github-pw" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserPassword "GitHub Password or Token: "

    // release on github
    Octokit.createClient user pw
    |> Octokit.createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes 
    |> Octokit.releaseDraft
    |> Async.RunSynchronously
)

// --------------------------------------------------------------------------------------
// Empty targets for readability

Target "Default" <| fun _ -> trace "Default target executed"
Target "Release" <| fun _ -> trace "Release target executed"
Target "CI" <| fun _ -> trace "CI target executed"

Target "Paket" <| fun _ -> trace "Paket should have been executed"

// --------------------------------------------------------------------------------------
// Target dependencies

"Clean"
    ==> "Build"
    ==> "Generate"
    ==> "Default"

let finalBinaries = "Default"

finalBinaries
    ==> "NuGet"
    ==> "CI"

finalBinaries
    ==> "GitRelease"
    ==> "GitHubRelease"
    ==> "Release"
    
finalBinaries
    ==> "NuGet"
    ==> "PublishNuget"
    ==> "Release"

RunTargetOrDefault "Default"
