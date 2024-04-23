# CalenderBooking

Example
- add 24/04/2024 12:00
- add 24/04 12:00
- delete 24/04 12:00
- find 24/04
- keep 12:00
  

Requirements
 - The application must accept the following commands from the command line:
 - ADD DD/MM hh:mm to add an appointment.
 - DELETE DD/MM hh:mm to remove an appointment.
 - FIND DD/MM to find a free timeslot for the day.
 - KEEP hh:mm keep a timeslot for any day.
 - The time slot will be always equal to 30 minutes.
 - The application can assign any slot on any day, with the following constraints:
 - The acceptable time is between 9AM and 5PM
 - Except from 4 PM to 5 PM on each second day of the third week of any month this must be reserved and unavailable
 - Use SQL Server Express LocalDB for the state storage.


Notes 
- UTC time not considered
- Only considered format DD/MM/YYYY hh:mm
- or DD/MM hh:mm ( the requirement says this???? ) will add 2024 to that

- IMPORTANT NOTE : KEEP hh:mm keep a timeslot for any day.
- asssuming this means. keep the time => book out the time. as only time is provided. find a timeslot and book. from now book available timeslot

Improvements:
 - Unit Testing
 - caching
 - Error handing.
 - More Testing. Way more testing
 - Removing restrictions over text and date input. better user experience.
