#!/bin/bash

dotnet restore ../src/Milvonion.Api/Milvonion.Api.csproj > last_run_log.txt

cd ..

dotnet publish -c Release -o ./artifacts/out >> ./build/last_run_log.txt

cd ./tests

dotnet test ../artifacts/out/Milvonion.UnitTests.dll --logger "html;logfilename=unitTestResults_$(date +"%Y_%m_%d_%I_%M_%p").html" >> ../build/last_run_log.txt
dotnet test ../artifacts/out/Milvonion.IntegrationTests.dll --logger "html;logfilename=integrationTestResults_$(date +"%Y_%m_%d_%I_%M_%p").html" >> ../build/last_run_log.txt

$SHELL