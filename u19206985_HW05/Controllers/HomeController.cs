using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using u19206985_HW05.Models.ViewModels;
using u19206985_HW05.Models;
using System.Web.Services.Description;

namespace u19206985_HW05.Controllers
{
    public class HomeController : Controller
    {
        //Connection Address
        SqlConnection connection = new SqlConnection("Data Source=.;Initial Catalog=Library;Integrated Security=True;");



        //Public Static list
        public static List<BookInfo> Books = new List<BookInfo>();
        public static List<Student> Students = new List<Student>();
        public static List<Borrow> Borrows = new List<Borrow>();
        public static List<BookInfo> Searched = null;
        public static int CheckingBook = 0;

        // GET: Home
        [HttpGet]
        public ActionResult Index()
        {
            List<BookInfo> returnBooks = null;
            try
            {
                clearLists();

                /**/
                if (Searched != null)
                {
                    returnBooks = Searched;
                }
                else
                {
                    getAllBooks();

                    /**/

                    getAllStudents();

                    /**/

                    getAllBorrows();

                    /**/

                    UpdateBooks();

                    returnBooks = Books;
                }


                /*Post Processing*/

                ViewBag.Types = GetTypes();

                ViewBag.Authors = GetAuthors();


            }
            catch (Exception message)
            {
                ViewBag.Message = message.Message;
            }
            finally
            {
                connection.Close();
            }

            return View(returnBooks);
        }

        [HttpPost]

        public ActionResult Search(string name, int? typeId, int? authorId)
        {
            try
            {
                if (Searched != null)
                {
                    Searched.Clear();
                }
                if (name != "" && typeId != null && authorId != null)
                {
                    //search has to be on all 3 parameters
                    getAllBooks();
                    Searched = Books.Where(x => x.name == name && x.typeId == typeId && x.authorId == authorId).ToList();
                }
                else if (name != "" && typeId != null && authorId == null)
                {
                    // search on name and type 
                }
                else if (name != "" && typeId == null && authorId != null)
                {
                    // search on name and author
                }
                else if (name == "" && typeId != null && authorId != null)
                {
                    //search on type and author
                }
                else if (name == "" && typeId == null && authorId != null)
                {
                    //search on author
                }
                else if (name == "" && typeId != null && authorId == null)
                {
                    // search on type
                    Searched = Books.Where(x => x.typeId == typeId).ToList();
                }
                else if (name != "" && typeId == null && authorId == null)
                {
                    // search on name 
                }
                else
                {
                    TempData["Message"] = "You Didnt Search For Anything Stop Wasting my Time !!!";
                }
            }
            catch (Exception message)
            {
                TempData["Message"] = message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Details(int bookId)
        {
            BookInfo ViewBook = new BookInfo();
            BookInfo bookInList = Books.Where(zz => zz.bookId == bookId).FirstOrDefault();

            if (bookInList != null)
            {
                var AllRecordsOfBookInBorrows = Borrows.Where(x => x.bookId == bookId).ToList();
                bookInList.totalBorrows = AllRecordsOfBookInBorrows.Count();
                List<BorrowsInfo> RecordOfBorrowed = new List<BorrowsInfo>();
                for (int i = 0; i < AllRecordsOfBookInBorrows.Count(); i++)
                {
                    BorrowsInfo record = new BorrowsInfo();
                    record.borrowId = AllRecordsOfBookInBorrows[i].borrowId;
                    record.studentName = Students.Where(x => x.studentId == AllRecordsOfBookInBorrows[i].studentId).FirstOrDefault().name;
                    record.takenDate = AllRecordsOfBookInBorrows[i].takenDate;
                    record.broughtDate = AllRecordsOfBookInBorrows[i].broughtDate;
                    RecordOfBorrowed.Add(record);
                }
                bookInList.borrowedRecords = RecordOfBorrowed;
            }
            else
            {
                ViewBag.Message = "Book Not Found";
            }
            return View(bookInList);
        }


        [HttpGet]
        public ActionResult ViewStudents(int bookId)
        {
            BookInfo book = Books.Where(x => x.bookId == bookId).FirstOrDefault();
            ViewBag.Status = book.status;
            if (book.studentId != 0)
            {
                ViewBag.studentId = book.studentId;
            }
            else
            {
                ViewBag.studentId = 0;
            }
            CheckingBook = 0;
            CheckingBook = bookId;
            return View(Students);
        }


        /*  METHODS -- METHODS -- METHODS */

        private void clearLists()
        {
            Books.Clear();
            Borrows.Clear();
        }

        private void getAllBooks()
        {
            SqlCommand getAllBooks = new SqlCommand("SELECT book.[bookId] as bookId ,book.[name] as name ,book.[pagecount] as pagecount ,book.[point] as point, auth.[surname] as authorSurname ,type.[name] typeName,  book.[authorId],book.[typeId] " +
                                        "FROM [Library].[dbo].[books] book " +
                                        "JOIN [Library].[dbo].[authors] auth on book.authorId = auth.authorId " +
                                        "JOIN [Library].[dbo].[types] type on book.typeId = type.typeId",
                                        connection);
            connection.Open();
            SqlDataReader readBooks = getAllBooks.ExecuteReader();
            while (readBooks.Read())
            {
                BookInfo book = new BookInfo();
                book.bookId = (int)readBooks["bookId"];
                book.name = (string)readBooks["name"];
                book.pagecount = (int)readBooks["pagecount"];
                book.point = (int)readBooks["point"];
                book.authorId = (int)readBooks["authorId"];
                book.typeId = (int)readBooks["typeId"];
                book.authorSurname = (string)readBooks["authorSurname"];
                book.typeName = (string)readBooks["typeName"];
                book.status = true;
                Books.Add(book);
            }
            connection.Close();
        }

        private void getAllStudents()
        {
            SqlCommand getAllStudents = new SqlCommand("SELECT * FROM [Library].[dbo].[students]", connection);
            connection.Open();
            SqlDataReader readStudents = getAllStudents.ExecuteReader();
            while (readStudents.Read())
            {
                Student student = new Student();
                student.studentId = (int)readStudents["studentId"];
                student.name = (string)readStudents["name"];
                student.surname = (string)readStudents["surname"];
                student.birthdate = (DateTime)readStudents["birthdate"];
                student.gender = (string)readStudents["gender"];
                student.Class = (string)readStudents["class"];
                student.point = (int)readStudents["point"];
                Students.Add(student);
            }
            connection.Close();
        }

        private void getAllBorrows()
        {
            SqlCommand getBorrows = new SqlCommand("SELECT * FROM [Library].[dbo].[borrows]", connection);
            connection.Open();
            SqlDataReader readBorrows = getBorrows.ExecuteReader();
            while (readBorrows.Read())
            {
                Borrow borrow = new Borrow();
                borrow.borrowId = (int)readBorrows["borrowId"];
                borrow.studentId = (int)readBorrows["studentId"];
                borrow.bookId = (int)readBorrows["bookId"];
                borrow.takenDate = Convert.ToDateTime(readBorrows["takenDate"]);
                var broughtDate = readBorrows["broughtDate"].ToString();
                if (broughtDate != "")
                {
                    borrow.broughtDate = Convert.ToDateTime(readBorrows["broughtDate"]);
                }
                else
                {
                    borrow.broughtDate = null;
                }

                Borrows.Add(borrow);
            }
            connection.Close();
        }

        private void UpdateBooks()
        {

            for (int i = 0; i < Borrows.Count; i++)
            {
                if (Borrows[i].broughtDate == null)
                {
                    BookInfo book = Books.Where(x => x.bookId == Borrows[i].bookId).FirstOrDefault();
                    book.status = false;
                    book.studentId = (int)Borrows[i].studentId;
                }
            }

        }


        private SelectList GetTypes()
        {
            List<TYPE> types = new List<TYPE>();
            SqlCommand getAllTypes = new SqlCommand("SELECT * FROM [Library].[dbo].[types]", connection);
            connection.Open();
            SqlDataReader readTypes = getAllTypes.ExecuteReader();
            while (readTypes.Read())
            {
                TYPE type = new TYPE();
                type.typeId = (int)readTypes["typeId"];
                type.name = (string)readTypes["name"];
                types.Add(type);
            }
            connection.Close();
            return new SelectList(types, "typeId", "name");
        }

        private SelectList GetAuthors()
        {
            List<Author> authors = new List<Author>();
            SqlCommand getAllAuthors = new SqlCommand("SELECT * FROM [Library].[dbo].[authors]", connection);
            connection.Open();
            SqlDataReader readAuthors = getAllAuthors.ExecuteReader();
            while (readAuthors.Read())
            {
                Author author = new Author();
                author.authorId = (int)readAuthors["authorId"];
                author.name = (string)readAuthors["name"];
                authors.Add(author);
            }
            return new SelectList(authors, "authorId", "name");
        }
    }

}
