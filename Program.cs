using CalenderBooking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Globalization;

// UTC time not considered
// Only conside format DD/MM/YYYY hh:mm
// or DD/MM hh:mm ( the requirement says this???? ) will add 2024 to that

// IMPORTANT NOTE : KEEP hh:mm keep a timeslot for any day.
// asssuming this means. keep the time => book out the time. as only time is provided. find a timeslot and book. from now book available timeslot

class Program
{
    private static DateTime date;

    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        Bookings = GetAllBookings();
        while (true)
        {
            var input_text = Console.ReadLine();

            if (!string.IsNullOrEmpty(input_text))
            {
                // break up text
                var input = input_text.Split(' ');

                var inp = input.Length > 2 ? input[2] : "";

                if (input.Length > 0)
                {
                    // check if the date input is correct
                    if (!DateTime.TryParse($"{input[1]} {inp}", out date))
                    {
                        if (!DateTime.TryParse($"{input[1]}/2024 {inp}", out date))
                        {
                            Console.WriteLine("Date Format not correct.");
                            continue;
                        }
                    }

                    var action = input[0];

                    switch (action.ToLower())
                    {
                        case "add":
                            AddBooking(date);
                            break;
                        case "delete":
                            DeleteBooking(date);
                            break;
                        case "find":
                            FindBooking(date);
                            break;
                        case "keep":
                            KeepBooking(date);
                            break;
                    }
                }
            }

            Console.WriteLine("\nCurrent Bookings\n");

            foreach ( var booking in Bookings )
            {
                Console.WriteLine($"{booking.DateTime} ");
            }
        }
    }

    private static List<Booking> Bookings { get; set; }

    private static void AddBooking(DateTime date)
    {
        if (!CanAddBooking(date))
        {
            return;
        }
        var booking = new Booking()
        {
            DateTime = date
        };
        using (var dbContext = new BookingContext())
        {
            var queryResult = dbContext.Booking.Add(booking);
            dbContext.SaveChanges();
        }

        // add to cache
        Bookings.Add(booking);

        Console.WriteLine($"\nBooking Added : {date}\n");
    }

    // The acceptable time is between 9AM and 5PM
    static TimeSpan start = new(9, 0, 0);
    // 9 am they can make a booking, but assuming 4:59 they cannot
    static TimeSpan end = new(16, 30, 0);

    // Except from 4 PM to 5 PM
    // 9 am they can make a booking, but assuming 3:59 they cannot
    // if 4 pm to 5 pm needs to be blocked, last booking made on the day should be 3:30
    static TimeSpan end_tues = new(15, 30, 0);

    private static bool CanAddBooking(DateTime date_time, bool is_console = true)
    {
        // check if its saturday or sunday
        if (date_time.DayOfWeek == DayOfWeek.Saturday || date_time.DayOfWeek == DayOfWeek.Sunday)
        {
            if (is_console)
            {
                Console.WriteLine("\nUnable to add booking. Weekdays only. Select another time.");
            }
            return false;
        }

        // check if the past, can't make bookings in the past
        if (date_time < DateTime.Now)
        {
            if (is_console)
            {
                Console.WriteLine("\nUnable to add booking. No past bookings. Select another time.");
            }
            return false;
        }

        if (date_time.TimeOfDay < start || date_time.TimeOfDay > end)
        {
            if (is_console)
            {
                Console.WriteLine("\nUnable to add booking. Booking can be made between 9am and 5 pm only. Select another time.");
            }
                return false;
        }

        // Except from 4 PM to 5 PM on each second day of the third week of any month - this must be reserved and unavailable
        // assuming second day means tuesday.
        if (date_time.DayOfWeek == DayOfWeek.Tuesday && date_time.Day > 14 && date_time.Day <= 21 && date_time.TimeOfDay > end_tues)
        {
            if (is_console)
            {
                Console.WriteLine("\nUnable to add booking. Booking reserved for this day. Select another time.");
            }
            return false;
        }

        // check for all values if booking already inserted
        foreach (var booking in Bookings )
        {
            var diff = (date_time - booking.DateTime).TotalMinutes;
            // The time slot will be always equal to 30 minutes.
            if (diff < 30 && diff > -30)
            {
                if (is_console)
                {
                    Console.WriteLine("\nUnable to add booking. Booking already exists. Select another time.");
                }
                return false;
            }
        }

        return true;
    }

    private static void DeleteBooking(DateTime date)
    {
        // chec if the value exists
        if (Bookings.Count(x => x.DateTime == date) == 0 )
        {
            Console.WriteLine("\nUnable to delete booking. Booking does not exist.");
        }
        
        // assuming to delete date and time needs to be exect. Even if reserved for 30 min
        using (var dbContext = new BookingContext())
        {
            dbContext.Booking.RemoveRange(dbContext.Booking.Where(x => x.DateTime == date));
            dbContext.SaveChanges();
        }

        // delete from cache
        Bookings.RemoveAll(x => x.DateTime == date);
    }

    private static List<Booking> GetAllBookings()
    {
        using (var dbContext = new BookingContext())
        {
            return dbContext.Booking.ToList();
        }
    }

    private static void FindBooking(DateTime date)
    {
        // FIND DD/MM to find a free timeslot for the day.

        // get all booking for that day

        var bookings = Bookings.Where(x => x.DateTime.Date == date.Date)?.ToList();

        // if null or zero, all day is available.
        if (bookings == null || bookings.Count == 0)
        {
            Console.WriteLine("\n All day available. Go crazy.");
            return;
        }

        var available_slots = new List<string>();

        // order the slots
        // from 9 am to first slot, check if greater that 30 mins
        // from sexond slot to next, check 30 mins
        // if last slot, check if 5 pm is more than 30 mins

        bookings = bookings.OrderBy(x => x.DateTime).ToList();

        var five_pm = new TimeSpan(15, 30, 0);

        for (int i = 0; i < bookings.Count; i++)
        {
            if (i == 0)
            {
                if ((bookings[i].DateTime.TimeOfDay - start).TotalMinutes > 30)
                {
                    available_slots.Add($"9:00 to {bookings[i].DateTime.ToString("HH:mm")}\n");
                }
            }
            else if (i == bookings.Count - 1)
            {
                if ((five_pm - bookings[i].DateTime.TimeOfDay).TotalMinutes > 30)
                {
                    available_slots.Add($"{bookings[i].DateTime.ToString("HH:mm")} to 17:00\n");
                }
            }
            else
            {
                // get next time
                var next = bookings[i+1].DateTime;
                if ((next.TimeOfDay - bookings[i].DateTime.TimeOfDay).TotalMinutes > 30)
                {
                    available_slots.Add($"{bookings[i].DateTime.ToString("HH:mm")} to {next.ToString("HH:mm")}\n");
                }
            }
        }

        // print all available times

        Console.WriteLine("\n The available times for this day are : \n");
        foreach (var x in  available_slots)
        {
            Console.WriteLine(x);
        }


    }

    private static void KeepBooking(DateTime date)
    {
        // add a booking for this time.

        var keep = DateTime.Now.Date + date.TimeOfDay;

        while (true)
        {
            if (CanAddBooking(keep, false))
            {
                AddBooking(keep);
                return;
            }
            else
            {
                keep = keep.AddDays(1);
                continue;
            }
        }
    }
}