#!/bin/bash

# VARIABLES
TARGETS=( "win-x64" "osx-x64" "linux-x64" "linux-arm")
RELEASEDIR="./release"
BASENAME="PKCS11Explorer"
BUILDTYPE="Release"
VERSIONNUMBER="0"

# START OF CODE
function Main()
{
	Setup
	#VerifyGitStatus
	GetCurrentVersion
	echo "Building version $VERSIONNUMBER"
	Build
}

function Setup()
{
	if [[ ! -d $RELEASEDIR ]];
	then
		mkdir $RELEASEDIR
	fi
}

function VerifyGitStatus()
{
	if ! git diff-index --quiet HEAD --; then
	    echo "Seems that some changes have not been commited. Please commit and try again."
	    exit 99
	fi
}

function GetCurrentVersion()
{
	if [[ $VERSIONNUMBER == "0" ]];
	then
		VERSIONNUMBER=`grep "ThisIsForScriptToFindVersionNumber" PKCS11Explorer/App.xaml.cs |cut -d"\"" -f2`
	fi
}

function Build()
{
	for TARGET in ${TARGETS[@]}; do
		echo "Building target $TARGET"
		CURRENTDIRNAME="$BASENAME"_"$BUILDTYPE"_"$TARGET"_"$VERSIONNUMBER"
		CURRENTOUTPUTDIR="$RELEASEDIR"/$CURRENTDIRNAME
		echo Output is $CURRENTOUTPUTDIR
		mkdir $CURRENTOUTPUTDIR
		dotnet publish -v m -r $TARGET -c $BUILDTYPE --self-contained true -f netcoreapp2.1 -o "`pwd`"/"$CURRENTOUTPUTDIR"/
		cd "$RELEASEDIR" && zip -r "$CURRENTDIRNAME".zip $CURRENTDIRNAME && cd -
		rm -rf $CURRENTOUTPUTDIR
	done
}

Main