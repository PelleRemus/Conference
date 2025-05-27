# Conference

The C# programs in this repository use high precision dlls, so in order to not get errors when trying to calculate anything using the programs, copy every file from the `DllPrecisionC#` directory, and paste it in the `bin\Debug\net9.0-windows` directory of the solution.

Unfortunately, by using this dlls, only the first execution of the current programs is accurate, as for the rest, the memory is corrupted and the calculations and results are completely wrong. so please, after running the calculations the first time, take in the results (or export them if the option is available) and close the program, running it again.