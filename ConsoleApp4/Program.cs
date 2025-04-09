using System;
using System.Collections.Generic;

namespace LibraryManager
{
    class Book
    {
        private static int _nextId = 1;
        public int BookId { get; private set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public bool IsAvailable { get; private set; } = true;
        public Member? Borrower { get; private set; }
        public List<Member> BorrowHistory { get; private set; } = new List<Member>();

        public Book(string title, string author)
        {
            BookId = _nextId++;
            Title = title;
            Author = author;
        }

        public void Borrow(Member member)
        {
            IsAvailable = false;
            Borrower = member;
            BorrowHistory.Add(member);
        }

        public void Return()
        {
            IsAvailable = true;
            Borrower = null;
        }

        public void DisplayBookInfo()
        {
            string status = IsAvailable ? "Available" : $"Not Available, Borrowed by: {Borrower?.Name}";
            Console.WriteLine($"Book ID: {BookId}, Title: {Title}, Author: {Author}, Status: {status}");
        }

        public void DisplayBorrowHistory()
        {
            Console.WriteLine($"Borrow history for '{Title}':");
            if (BorrowHistory.Count == 0)
            {
                Console.WriteLine("No borrow history.");
            }
            else
            {
                foreach (var member in BorrowHistory)
                {
                    Console.WriteLine($"- {member.Name} (Member ID: {member.MemberId})");
                }
            }
        }
    }

    class Member
    {
        private static int _nextId = 1;
        public int MemberId { get; private set; }
        public string Name { get; set; }
        public List<Book> BorrowedBooks { get; private set; }

        public Member(string name)
        {
            MemberId = _nextId++;
            Name = name;
            BorrowedBooks = new List<Book>();
        }

        public void BorrowBook(Book book)
        {
            if (!book.IsAvailable)
            {
                Console.WriteLine($"'{book.Title}' is already checked-in.");
                return;
            }

            book.Borrow(this);
            BorrowedBooks.Add(book);
            Console.WriteLine($"{Name} checked-in '{book.Title}'.");
        }

        public void ReturnBook(Book book)
        {
            if (BorrowedBooks.Remove(book))
            {
                book.Return();
                Console.WriteLine($"{Name} returned '{book.Title}'.");
            }
            else
            {
                Console.WriteLine($"{Name} did NOT check out '{book.Title}'.");
            }
        }

        public void DisplayMemberInfo()
        {
            Console.WriteLine($"Member ID: {MemberId}, Name: {Name}");
        }
    }

    class Library
    {
        private static Library? _instance;
        private List<Book> books = new List<Book>();
        private List<Member> members = new List<Member>();

        private Library() { }

        public static Library Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Library();
                }
                return _instance;
            }
        }

        public void AddBook(string title, string author)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (author == null) throw new ArgumentNullException(nameof(author));
            books.Add(new Book(title, author));
            Console.WriteLine("Book added successfully.");
        }

        public void EditBook()
        {
            if (books.Count == 0)
            {
                Console.WriteLine("No books available to edit.");
                return;
            }

            DisplayLibrary();
            Console.Write("Enter Book ID to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId)) return;

            var book = books.Find(b => b.BookId == bookId);
            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            Console.Write("Enter new title (leave blank to keep current): ");
            string newTitle = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newTitle)) book.Title = newTitle;

            Console.Write("Enter new author (leave blank to keep current): ");
            string newAuthor = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newAuthor)) book.Author = newAuthor;

            Console.WriteLine("Book details updated successfully.");
        }

        public void AddMember(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            members.Add(new Member(name));
            Console.WriteLine("Member added successfully.");
        }

        public void EditMember()
        {
            if (members.Count == 0)
            {
                Console.WriteLine("No members available to edit.");
                return;
            }

            DisplayMembers();
            Console.Write("Enter Member ID to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId)) return;

            var member = members.Find(m => m.MemberId == memberId);
            if (member == null)
            {
                Console.WriteLine("Member not found.");
                return;
            }

            Console.Write("Enter new name (leave blank to keep current): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName)) member.Name = newName;

            Console.WriteLine("Member details updated successfully.");
        }

        public void SearchBook(string keyword)
        {
            if (keyword == null) throw new ArgumentNullException(nameof(keyword));
            var results = books.FindAll(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                             b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (results.Count > 0)
            {
                Console.WriteLine("Search Results:");
                foreach (var book in results) book.DisplayBookInfo();
            }
            else
            {
                Console.WriteLine("No books found.");
            }
        }

        public void DisplayLibrary()
        {
            Console.WriteLine("Library Books:");
            foreach (var book in books) book.DisplayBookInfo();
        }

        public void DisplayMembers()
        {
            Console.WriteLine("Library Members:");
            foreach (var member in members) member.DisplayMemberInfo();
        }

        public void DisplayBorrowHistory(int bookId)
        {
            var book = FindBook(bookId);
            if (book != null)
            {
                book.DisplayBorrowHistory();
            }
            else
            {
                Console.WriteLine("Book not found.");
            }
        }

        public void DisplayMembersWithBorrowedBooks()
        {
            Console.WriteLine("Members with Borrowed Books:");
            foreach (var member in members)
            {
                if (member.BorrowedBooks.Count > 0)
                {
                    Console.WriteLine($"Member ID: {member.MemberId}, Name: {member.Name}");
                    foreach (var book in member.BorrowedBooks)
                    {
                        Console.WriteLine($"  - Book ID: {book.BookId}, Title: {book.Title}, Author: {book.Author}");
                    }
                }
            }
        }

        public Book? FindBook(int bookId) => books.Find(b => b.BookId == bookId);
        public Member? FindMember(int memberId) => members.Find(m => m.MemberId == memberId);
    }

    interface IMenuStrategy
    {
        void Execute();
    }

    class BookManager : IMenuStrategy
    {
        private Library library = Library.Instance;

        public void Execute()
        {
            while (true)
            {
                Console.WriteLine("\nBook Management Menu:");
                Console.WriteLine("1. Add Book");
                Console.WriteLine("2. Edit Book");
                Console.WriteLine("3. View Books");
                Console.WriteLine("4. View Borrow History");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Select an option: ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 5)
                {
                    Console.WriteLine("Invalid choice. Try again.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine() ?? string.Empty;
                        Console.Write("Enter author: ");
                        string author = Console.ReadLine() ?? string.Empty;
                        library.AddBook(title, author);
                        break;
                    case 2:
                        library.EditBook();
                        break;
                    case 3:
                        library.DisplayLibrary();
                        break;
                    case 4:
                        Console.Write("Enter Book ID to view borrow history: ");
                        if (int.TryParse(Console.ReadLine(), out int bookId))
                        {
                            library.DisplayBorrowHistory(bookId);
                        }
                        break;
                    case 5:
                        return;
                }
            }
        }
    }

    class MemberManager : IMenuStrategy
    {
        private Library library = Library.Instance;

        public void Execute()
        {
            while (true)
            {
                Console.WriteLine("\nMember Management Menu:");
                Console.WriteLine("1. Add Member");
                Console.WriteLine("2. Edit Member");
                Console.WriteLine("3. View Members");
                Console.WriteLine("4. Back to Main Menu");
                Console.Write("Select an option: ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 4)
                {
                    Console.WriteLine("Invalid choice. Try again.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Write("Enter member name: ");
                        string name = Console.ReadLine() ?? string.Empty;
                        library.AddMember(name);
                        break;
                    case 2:
                        library.EditMember();
                        break;
                    case 3:
                        library.DisplayMembers();
                        break;
                    case 4:
                        return;
                }
            }
        }
    }

    class CheckInOutManager : IMenuStrategy
    {
        private Library library = Library.Instance;

        public void Execute()
        {
            while (true)
            {
                Console.WriteLine("\nCheck-In/Check-Out Menu:");
                Console.WriteLine("1. Check-In Book");
                Console.WriteLine("2. Check-Out Book");
                Console.WriteLine("3. Back to Main Menu");
                Console.Write("Select an option: ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 3)
                {
                    Console.WriteLine("Invalid choice. Try again.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        library.DisplayLibrary();
                        Console.Write("Enter book ID to check-in: ");
                        if (int.TryParse(Console.ReadLine(), out int bookId))
                        {
                            library.DisplayMembers();
                            Console.Write("Enter your member ID: ");
                            if (int.TryParse(Console.ReadLine(), out int memberId))
                            {
                                library.FindMember(memberId)?.BorrowBook(library.FindBook(bookId));
                            }
                        }
                        break;
                    case 2:
                        library.DisplayLibrary();
                        Console.Write("Enter book ID to check-out: ");
                        if (int.TryParse(Console.ReadLine(), out int returnBookId))
                        {
                            library.DisplayMembers();
                            Console.Write("Enter your member ID: ");
                            if (int.TryParse(Console.ReadLine(), out int returnMemberId))
                            {
                                library.FindMember(returnMemberId)?.ReturnBook(library.FindBook(returnBookId));
                            }
                        }
                        break;
                    case 3:
                        return;
                }
            }
        }
    }

    class SearchBookManager : IMenuStrategy
    {
        private Library library = Library.Instance;

        public void Execute()
        {
            Console.Write("Enter search keyword: ");
            string keyword = Console.ReadLine() ?? string.Empty;
            library.SearchBook(keyword);
        }
    }

    class Program
    {
        static void Main()
        {
            Library library = Library.Instance;

            while (true)
            {
                Console.WriteLine("\nLibrary Management Menu:");
                Console.WriteLine("1. Book Manager");
                Console.WriteLine("2. Member Manager");
                Console.WriteLine("3. Check-In/Check-Out");
                Console.WriteLine("4. Search Book");
                Console.WriteLine("5. View  Members with Check-in Books");
                Console.WriteLine("6. Exit");
                Console.Write("Select an option: ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 6)
                {
                    Console.WriteLine("Invalid choice. Try again.");
                    continue;
                }

                IMenuStrategy? strategy = choice switch
                {
                    1 => new BookManager(),
                    2 => new MemberManager(),
                    3 => new CheckInOutManager(),
                    4 => new SearchBookManager(),
                    5 => null,
                    6 => null,
                    _ => null
                };

                if (choice == 5)
                {
                    library.DisplayMembersWithBorrowedBooks();
                }
                else if (strategy == null)
                {
                    return;
                }
                else
                {
                    strategy.Execute();
                }
            }
        }
    }
}