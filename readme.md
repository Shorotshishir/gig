# gig

made this to easily create .gitignore file in any systems.  
built on Linux with dotnet 8.0.

As it is `self contained` and `single-file` , No dotnet installation is required

gitignore of this project is created with this tool !!

## Usage

- create a local git repository with

```bash
git init
```

- use gig to create gitignore for preferred language or common framework like below

```bash
# for cpp
gig cpp
# for unity
gig unity 
# for dotnet 
gig dotnet
```

- to get the full list of type command

```bash
gig
# or 

gig help
```

## Build project

required to build : `dotnet 8.0`

## Linux

- publish release build (self contained , single file)

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

- make executable

```bash
chmod +x bin/Release/net8.0/linux-x64/publish/gig
```

- copy to correct place for system-wide terminal access

```bash
sudo cp  bin/Release/net8.0/linux-x64/publish/gig /usr/local/bin/
```

## Windows

- publish release build (self contained , single file)

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

- copy the build to `C:\bin`
- add `C:\bin` to environment path
