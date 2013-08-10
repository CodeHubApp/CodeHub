using System.Collections.Generic;
using MonoTouch;
using System.Collections;
using System.Linq;
using SQLite;

namespace CodeHub.Data
{
	/// <summary>
	/// A collection of accounts within the system
	/// </summary>
	public class Accounts : IEnumerable<Account>
	{
		private SQLiteConnection _userDatabase;
		private static Account _activeAccount;

        public Account ActiveAccount
		{
			get { return _activeAccount; }
			set { _activeAccount = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeFramework.Data.Accounts`1"/> class.
		/// </summary>
		/// <param name="userDatabase">User database.</param>
		public Accounts(SQLiteConnection userDatabase)
		{
			_userDatabase = userDatabase;
		}

		/// <summary>
		/// Gets the count of accounts in the database
		/// </summary>
		public int Count 
		{
            get { return _userDatabase.Table<Account>().Count(); }
		}

		/// <summary>
		/// Gets the default account
		/// </summary>
        public Account GetDefault()
		{
			var id = Utilities.Defaults.IntForKey("DEFAULT_ACCOUNT");
            return _userDatabase.Table<Account>().SingleOrDefault(x => x.Id == id);
		}

		/// <summary>
		/// Sets the default account
		/// </summary>
        public void SetDefault(Account account)
		{
			if (account == null)
				Utilities.Defaults.RemoveObject("DEFAULT_ACCOUNT");
			else
				Utilities.Defaults.SetInt(account.Id, "DEFAULT_ACCOUNT");
			Utilities.Defaults.Synchronize();
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
        public IEnumerator<Account> GetEnumerator ()
		{
            return _userDatabase.Table<Account>().GetEnumerator();
		}

		/// <summary>
		/// Insert the specified account.
		/// </summary>
        public void Insert(Account account)
		{
			_userDatabase.Insert(account);
		}

		/// <summary>
		/// Remove the specified account.
		/// </summary>
        public void Remove(Account account)
		{
			_userDatabase.Delete(account);
		}

		/// <summary>
		/// Update this instance in the database
		/// </summary>
        public void Update(Account account)
		{
			_userDatabase.Update(account);
		}

		/// <summary>
		/// Remove the specified username.
		/// </summary>
		public void Remove(string username)
		{
            var q = from f in _userDatabase.Table<Account>()
				where f.Username == username
					select f;
			var account = q.FirstOrDefault();
			if (account != null)
				Remove(account);
		}

		/// <summary>
		/// Checks to see whether a specific account exists (Username comparison)
		/// </summary>
        public bool Exists(Account account)
		{
			return Find(account.Username) != null;
		}

		/// <summary>
		/// Find the specified account via it's username
		/// </summary>
        public Account Find(string username)
		{
            var query = _userDatabase.Query<Account>("select * from Account where LOWER(Username) = LOWER(?)", username);
			if (query.Count > 0)
				return query[0];
			return null;
		}
	}

}

