using Ramirez_Mackenzie_HW5.DAL;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Ramirez_Mackenzie_HW5.Models;
using System.Text;
using System.Threading.Tasks;

namespace Ramirez_Mackenzie_HW5.Utilities
{
    public static class OrderNumberGenerator
    {
        public static Int32 GetNextOrderNumber(AppDbContext _context)
        {
            //Set a number where the course numbers should start
            const Int32 START_NUMBER = 3000;

            Int32 intMaxCourseNumber; //the current maximum course number
            Int32 intNextCourseNumber; //the course number for the next class

            if (_context.Products.Count() == 0) //there are no courses in the database yet
            {
                intMaxCourseNumber = START_NUMBER; //course numbers start at 3001
            }
            else
            {
                intMaxCourseNumber = _context.Products.Max(c => c.ProductID); //this is the highest number in the database right now
            }

            //You added courses before you realized that you needed this code
            //and now you have some course numbers less than 3000
            if (intMaxCourseNumber < START_NUMBER)
            {
                intMaxCourseNumber = START_NUMBER;
            }

            //add one to the current max to find the next one
            intNextCourseNumber = intMaxCourseNumber + 1;

            //return the value
            return intNextCourseNumber;
        }

    }
}
