@ECHO OFF
REM This must be run from the project root folder (parent of lib, doc, source, tools)
REM If you don't know this, you likely aren't supposed to be releasing dasBlog, eh?

@FOR %%i IN (docs, lib, source, tools) DO @IF NOT EXIST %%i GOTO :ERROR_WRONG_FOLDER

REM Determine build type (useful when debugging this batch file)
SET BUILDTYPE=%1
SET BUILDWEB=0
SET BUILDSRC=0

IF /i "X%BUILDTYPE%" EQU "X" SET BUILDTYPE=ALL

IF /i "%BUILDTYPE%" == "ALL" SET BUILDWEB=1 & SET BUILDSRC=1
IF /i "%BUILDTYPE%" == "WEB" SET BUILDWEB=1
IF /i "%BUILDTYPE%" == "SOURCE" SET BUILDSRC=1

IF "%BUILDWEB%%BUILDSRC%" == "00" GOTO :ERROR_INVALID_PARAMETER
@ECHO ON

REM Get build version from AssemblyInfo.cs - make sure you update that file first.
@FOR /F "usebackq skip=2 tokens=1,2 delims=()" %%u in (`FIND.EXE /i "assemblyversion" source\assemblyinfo.cs`) DO (SET DASVERSION=%%~v)
SET DASBLOGFILE=dasBlog-%DASVERSION%-Web-Files.zip
SET DASBLOGSRCFILE=dasBlog-%DASVERSION%-Source.zip

:CLEAN
IF EXIST build rd /s /q build
REM @GOTO :SOURCE_RELEASE


:BINARY_RELEASE
IF %BUILDWEB% NEQ 1 GOTO :SOURCE_RELEASE

MKDIR build\dasblogce
xcopy source\DasBlogUpgrader\bin\Debug\*.* "build\upgradedasblog\" /s
xcopy source\newtelligence.DasBlog.Web\SiteConfig\*.* "build\dasblogce\SiteConfig\" /s
del build\dasblogce\SiteConfig\site.config.deploy
del build\dasblogce\SiteConfig\siteSecurity.config.deploy
copy /y source\newtelligence.DasBlog.Web\SiteConfig\site.config.deploy build\dasblogce\SiteConfig\site.config
copy /y source\newtelligence.DasBlog.Web\SiteConfig\siteSecurity.config.deploy build\dasblogce\SiteConfig\siteSecurity.config
copy source\newtelligence.DasBlog.Web\*.webinfo build\dasblogce
copy source\newtelligence.DasBlog.Web\*.aspx build\dasblogce
copy source\newtelligence.DasBlog.Web\*.ascx build\dasblogce
copy source\newtelligence.DasBlog.Web\web.config build\dasblogce
copy source\newtelligence.DasBlog.Web\*.asax build\dasblogce
copy source\newtelligence.DasBlog.Web\*.asmx build\dasblogce
copy source\newtelligence.DasBlog.Web\*.licx build\dasblogce
copy source\newtelligence.DasBlog.Web\*.xml build\dasblogce
xcopy source\newtelligence.DasBlog.Web\*.dll "build\dasblogce" /s
xcopy source\newtelligence.DasBlog.Web\*.pdb "build\dasblogce" /s
xcopy source\newtelligence.DasBlog.Web\ftb\*.* "build\dasblogce\ftb\" /s
xcopy source\newtelligence.DasBlog.Web\images\*.* "build\dasblogce\images\" /s
xcopy source\newtelligence.DasBlog.Web\scripts\*.* "build\dasblogce\scripts\" /s
xcopy source\newtelligence.DasBlog.Web\content\*.deploy "build\dasblogce\content\" /s
ren build\dasblogce\content\*.deploy *.
xcopy source\newtelligence.DasBlog.Web\themes\blogxp "build\dasblogce\themes\blogxp\" /s
xcopy source\newtelligence.DasBlog.Web\themes\calmblue "build\dasblogce\themes\calmblue\" /s
xcopy source\newtelligence.DasBlog.Web\themes\candidblue "build\dasblogce\themes\candidblue\" /s
xcopy source\newtelligence.DasBlog.Web\themes\dasBlog "build\dasblogce\themes\dasBlog\" /s
xcopy source\newtelligence.DasBlog.Web\themes\dasblogger "build\dasblogce\themes\dasblogger\" /s
xcopy source\newtelligence.DasBlog.Web\themes\dasblueblog "build\dasblogce\themes\dasblueblog\" /s
xcopy source\newtelligence.DasBlog.Web\themes\dasEmerald "build\dasblogce\themes\dasEmerald\" /s
xcopy source\newtelligence.DasBlog.Web\themes\directionalredux "build\dasblogce\themes\directionalredux\" /s
xcopy source\newtelligence.DasBlog.Web\themes\discreetBlogBlue "build\dasblogce\themes\discreetBlogBlue\" /s
xcopy source\newtelligence.DasBlog.Web\themes\elegante "build\dasblogce\themes\elegante\" /s
xcopy source\newtelligence.DasBlog.Web\themes\essence "build\dasblogce\themes\essence\" /s
xcopy source\newtelligence.DasBlog.Web\themes\justhtml "build\dasblogce\themes\justhtml\" /s
xcopy source\newtelligence.DasBlog.Web\themes\mads_simple "build\dasblogce\themes\mads_simple\" /s
xcopy source\newtelligence.DasBlog.Web\themes\mobile "build\dasblogce\themes\mobile\" /s
xcopy source\newtelligence.DasBlog.Web\themes\business "build\dasblogce\themes\business\" /s
xcopy source\newtelligence.DasBlog.Web\themes\mono "build\dasblogce\themes\mono\" /s
xcopy source\newtelligence.DasBlog.Web\themes\movableRadioBlue "build\dasblogce\themes\movableRadioBlue\" /s
xcopy source\newtelligence.DasBlog.Web\themes\movableRadioHeat "build\dasblogce\themes\movableRadioHeat\" /s
xcopy source\newtelligence.DasBlog.Web\themes\nautica022 "build\dasblogce\themes\nautica022\" /s
xcopy source\newtelligence.DasBlog.Web\themes\orangeCream "build\dasblogce\themes\orangeCream\" /s
xcopy source\newtelligence.DasBlog.Web\themes\Portal "build\dasblogce\themes\Portal\" /s
xcopy source\newtelligence.DasBlog.Web\themes\Project84 "build\dasblogce\themes\Project84\" /s
xcopy source\newtelligence.DasBlog.Web\themes\Project84Grass "build\dasblogce\themes\Project84Grass\" /s
xcopy source\newtelligence.DasBlog.Web\themes\slate "build\dasblogce\themes\slate\" /s
xcopy source\newtelligence.DasBlog.Web\themes\soundWaves "build\dasblogce\themes\soundWaves\" /s
xcopy source\newtelligence.DasBlog.Web\themes\tricoleur "build\dasblogce\themes\tricoleur\" /s
xcopy source\newtelligence.DasBlog.Web\themes\useit "build\dasblogce\themes\useit\" /s
xcopy source\newtelligence.DasBlog.Web\themes\Voidclass2 "build\dasblogce\themes\Voidclass2\" /s

xcopy source\newtelligence.DasBlog.Web\smilies\*.* "build\dasblogce\smilies\" /s
xcopy source\newtelligence.DasBlog.Web\DatePicker\*.* "build\dasblogce\DatePicker\" /s

del build\dasblogce\.#* /q /s
del build\upgradedasblog\.#* /q /s
del build\dasblogce\.cvsignore /q /s
del build\upgradedasblog\.cvsignore /q /s

del build\dasblogce\bin\FredCK.FCKeditorV2.* /q
del build\dasblogce\bin\newtelligence.DasBlog.Contrib.FCKeditor.* /q
rd /s /q build\dasblogce\FCKeditor
del build\dasblogce\SiteConfig\FCK*.* /q

MKDIR build\dasblogce\logs
IF NOT EXIST build\dasblogce\content\binary (MKDIR build\dasblogce\content\binary)

pushd .
CD build
..\tools\PACOMP.EXE -a -r -P -q %DASBLOGFILE% dasblogce\*.*
..\tools\PACOMP.EXE -a -r -P -q %DASBLOGFILE% upgradedasblog\*.*
..\tools\PACOMP.EXE -a -q %DASBLOGFILE% ..\tools\CreateDasBlogVdir.vbs
..\tools\PACOMP.EXE -a -q %DASBLOGFILE% ..\*.xml
..\tools\PACOMP.EXE -a -q %DASBLOGFILE% ..\docs\*.*
popd

REM RMDIR /s /q build\dasblogce
REM RMDIR /s /q build\upgradedasblog

:SOURCE_RELEASE
IF %BUILDSRC% NEQ 1 GOTO :PACKAGE_COMPLETE

REM Now the source
MKDIR build\source
xcopy source\*.* /e /y build\source

pushd .
cd build\source
@FOR /R %%x IN (bin, obj, debug, release) DO @IF EXIST %%x (
@echo Deleting %%x
@RMDIR /s /q %%x
)

REM call ..\..\tools\cleansource .
del *.bat /q /s
del pacomp.exe /q /s
del *.zip /q /s
del *.suo /q /s
del *.log /q /s
del *.csproj.user /q /s
del *.patch /q /s

REM Weird issue with PACOMP - won't zip the empty logs directory unless we re-create it here
rd /s /q newtelligence.DasBlog.Web\logs
md newtelligence.DasBlog.Web\logs

rd /s /q newtelligence.DasBlog.Web\FCKeditor
del newtelligence.DasBlog.Web\SiteConfig\FCK*.* /q

del *.backup /q /s
del .#* /q /s
del .cvsignore /q /s
del newtelligence.DasBlog.Web\content\*.xml /q
copy /y newtelligence.DasBlog.Web\content\*.deploy newtelligence.DasBlog.Web\content\*.
del newtelligence.DasBlog.Web\content\binary /q
copy /y newtelligence.DasBlog.Web\SiteConfig\site.config.deploy newtelligence.DasBlog.Web\SiteConfig\site.config
copy /y newtelligence.DasBlog.Web\SiteConfig\siteSecurity.config.deploy newtelligence.DasBlog.Web\SiteConfig\siteSecurity.config

popd

pushd .
cd build
..\tools\PACOMP.EXE -a -r -P -q %DASBLOGSRCFILE% source\*.*
popd 
tools\PACOMP.EXE -a -r -P -q build\%DASBLOGSRCFILE% lib\*.*
tools\PACOMP.EXE -a -r -P -q build\%DASBLOGSRCFILE% tools\*.*
tools\PACOMP.EXE -a -r -P -q build\%DASBLOGSRCFILE% docs\*.*
tools\PACOMP.EXE -a -q build\%DASBLOGSRCFILE% package.bat
REM rd /s /q source

@GOTO :PACKAGE_COMPLETE

:PACKAGE_COMPLETE
@ECHO Packaging %BUILDTYPE% complete.
@GOTO :EOF

:ERROR_WRONG_FOLDER
@ECHO The current folder must have  docs, lib, source, and tools subfolders
@GOTO :EOF

:ERROR_INVALID_PARAMETER
@ECHO Invalid build type. Specify ALL, WEB, or SOURCE
@GOTO :EOF
