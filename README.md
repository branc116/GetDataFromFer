# Download all your fer data from one place and keep it in sync
## How it works
* It checks ferWeb for list of files, then it checks local directory for the same files.
* If it doesn't fine the file localy it will be downloaded and saved localy.

## How to run
* Download binary from somewhare.
* Build it yourself:
  * Download dotnet core 2.0
  * > `git clone https://github.com/branc116/GetDataFromFer`
  * > `cd GetDataFromFer`
  * > `dotnet restore`
  * > `dotnet build`
  * > [Add envirement variable](https://www.schrodinger.com/kb/1842) called "FERCMS" with value of cms cookie which you can find when you login to [intranet](www.fer.unizg.hr/intranet)
* ![Step 1](Docs/Screenshot (9).png)
* ![Step 2](Docs/Screenshot (10).png)
  * > `dotnet run [List of classes you want to sync] [output directory]`
    * eg `dotnet run os mat3 mat3r C:\FER\semestar3`
