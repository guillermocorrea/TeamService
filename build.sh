dotnet restore
dotnet build

cd TeamService.Tools

# Instrument assemblies inside 'test' folder to detect hits for source files inside 'src' folder
dotnet minicover instrument --workdir ../ --assemblies test/**/bin/**/*.dll --sources TeamService/**/*.cs 

# Reset hits count in case minicover was run for this project
dotnet minicover reset

cd ..

for project in test/**/*.csproj; do dotnet test --no-build $project; done

cd TeamService.Tools

# Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
dotnet minicover uninstrument --workdir ../

# Create html reports inside folder coverage-html
dotnet minicover htmlreport --workdir ../ --threshold 85

# Print console report
# This command returns failure if the coverage is lower than the threshold
dotnet minicover report --workdir ../ --threshold 85

cd ..