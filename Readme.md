# Congestion Tax Calculator

## Structure
I've choosen to build the application as a .net REST API in a 3-layer structure. <br />
<b>API.CongestionTax.Web</b> is the entry point with REST Controllers. <br /> <br />
<b>API.CongestionTax.Business</b> handles business logic and this is where I decided to place the file CongestionTaxCalculator.cs that calculates the taxes. <br /> <br />
<b>API.CongestionTax.Data</b> is the layer with dependencies to other systems and which make external calls. Here I've placed a file MockedDatabaseProvider.cs. This file is prepared to make calls to an external database but becouse of limited time and not wanting to host a database somewhere unreliable I've just mocked it with data from the original test-project. <br />
For an example of how I would normaly create a mysql database provider, please see https://github.com/Stafffe/API.Customer/blob/master/API.Customer.Data/Providers/DatabaseProvider.cs. <br /> <br />

## Somethings I decided to rebuild
<b>Vehicle Types:</b> I noticed that the different vehicles where using some weird polymorphism depending on an interface. As the application is now it is really not necessary and I replaced that whole structure with an enum instead. An alternative solution would have been to keep it as it was and use different endpoints for the different types ex: GetTaxForBus, GetTaxForMotorBike etc. This could be better if more vehicle specific data/logic would be added later on. Either way the string comparision of types was terrible and very open for mistakes for example the missmatch that existed in the code for motorbikes/motorcycles. Instead this should have been replaced with a type comparision atleast. <br /><br />
<b>CongestionTaxCalculator:</b> This file was hard to read and filled with buggs. I cleaned it up from scratch to make the code easier to read and more resilient to coding mistakes in the future. Also added unit tests for it to make sure the different parts works correctly.

## Trying it out
Start the application. <br />
The swagger shows up and the application listens to gets for url /VehicleTax. <br />
There is a VehicleType that means which vehicle that is being taxed. Motorcycle = 1, Tractor = 2, Emergency = 3, Diplomat = 4, Foreign = 5, Military = 6, Bus = 7, Car = 8, Other = 9 <br />
You can add occurances for datesForTaxations with format YYYY-MM-DD HH:mm:ss.

## Points of improvement
<b>Mapping:</b> As of now I've been very lazy with mapping between the projects. API.CongestionTax.Webs endpoint data objects should not be used in for example API.CongestionTax.Business. Dependencies on the same dataobjects between the layers like this can have frustrating consequences on code in the future and should be avoided. Instead they should have different data objects with mapping between. <br />
<b>Real database:</b> I did not feel like building and hosting a database. Was lazy and just returned mocked data that matches the original test-code. If desired I could in the future make a database if that would prove my competence.<br />
<b>Swagger documentation:</b> I've put minimal effort into the swagger documentation/the url-structure. It could probably be improved. <br />
<b>Tests:</b> I've only put tests on the main file CongestionTaxCalculator.cs. This should be extended to the whole project in a real application. Some integration tests might be nice aswell<br />
<b>Validations:</b> Could probably add better support for wrongly inputed data to the api. As of now there is where little validations. <br />
<b>More customability:</b> Could add more customability between cities. For example do we always want to allow tax-free days the day before holiday? Or always free on Saturday/Sunday?

## Buggs i found
I saw that a string comparision for vehicle types was off. Motorbike was written Motorcycle in a different part of the code which made it never match. Seems to me like they are the same thing. <br />
Taxations interval was wrongly calculated and would have resulted in really weird output if not changed. <br />
Time comparision was wrongly written with .millisecounds which would not result in correct calculations. <br />
Probably more, the code was written in a way that left it easily open for mistakes so i refactored it and in the process the buggs should be fixed.

## My questions
The requirement of the application does not say anything about tractors being tax-free but the code did. This is something I would talk with a product owner about. <br /><br />
I do not quite understand how the time intervalls are supoused to work. I have two questions about the subject. If I have 3 occurences within 60mins and then another one 5mins after, does that group itself with the first 3 as 1 intervall and the last one alone? Or can tax-occurences overlap for 2 different time-intervals? I would quess that they can't, but I'm not sure.<br />
The other question I have would be if the first occurence was tax-free. Would that start a time-interval? Lets say I have a time interval starting with a tax-free occurance followed by a tax-requiering occurance 30mins later followed by another tax-occurance 40min later. Would the person in this case be taxed once or twice? I have personally no idea so would have liked to talk with a product owner for this case. In my code I decided to place it as 2 taxes.<br /><br />
What was the dates for? I quess it had something to do with testing of the application. 