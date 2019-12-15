format:
	cd Example && dotnet format && cd -
	cd Example.Tests && dotnet format && cd -

test:
	cd Example.Tests && dotnet test && cd -

clean:
	cd Example && dotnet clean && cd -
	cd Example.Tests && dotnet clean && cd -

purge: clean
	cd Example && rm -rf bin obj && cd -
	cd Example.Tests && rm -rf bin obj && cd -
