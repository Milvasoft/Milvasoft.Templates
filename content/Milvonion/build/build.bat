dotnet restore ../src/Milvonion.Api/Milvonion.Api.csproj > last_run_log.txt

@echo off

cd ..

@echo on

dotnet publish ./src/Milvonion.Api/Milvonion.Api.csproj -c Release -o ./artifacts/out >> ./build/last_run_log.txt

@echo off

cd ./tests

@echo off

set CUR_YYYY=%date:~10,4%
set CUR_MM=%date:~4,2%
set CUR_DD=%date:~7,2%
set CUR_HH=%time:~0,2%
if %CUR_HH% lss 10 (set CUR_HH=0%time:~1,1%)

set CUR_NN=%time:~3,2%
set CUR_SS=%time:~6,2%
set CUR_MS=%time:~9,2%

set SUBFILENAME=%CUR_YYYY%%CUR_MM%%CUR_DD%-%CUR_HH%%CUR_NN%%CUR_SS%

@echo on

dotnet test ../artifacts/out/Milvonion.UnitTests.dll --logger "html;logfilename=unitTestResults_%SUBFILENAME%.html" >> ../build/last_run_log.txt
dotnet test ../artifacts/out/Milvonion.IntegrationTests.dll --logger "html;logfilename=integrationTestResults_%SUBFILENAME%.html" >> ../build/last_run_log.txt

cmd/k