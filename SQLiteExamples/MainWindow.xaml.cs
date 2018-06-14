using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace SQLiteExamples
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Creates example.db database file on base directory
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CreateDb_Click(object sender, RoutedEventArgs e)
		{
			const string fileName = "example.db";

			if (File.Exists(fileName))
			{
				MessageBox.Show($"Your database {fileName} already exists", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			else
			{
				// create database file
				SQLiteConnection.CreateFile(fileName);

				if (File.Exists(fileName))
				{
					MessageBox.Show($"Your database {fileName} created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					MessageBox.Show($"Your database {fileName} could not created", "Fail", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		/// <summary>
		/// Insert some value into database
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void InsertSomeValues(object sender, RoutedEventArgs e)
		{
			var products = new List<Product>();

			#region Some Products

			var newProduct = new Product()
			{
				Manufacturer = "Windows",
				Name = "Windows 10 Pro",
				Version = 10,
				Count = 1,
				Price = 999
			};
			products.Add(newProduct);

			newProduct = new Product()
			{
				Manufacturer = "Mac OS",
				Name = "Mac OS X",
				Version = 10,
				Count = 1,
				Price = 999
			};
			products.Add(newProduct);

			newProduct = new Product()
			{
				Manufacturer = "Linux",
				Name = "Debian",
				Version = 10,
				Count = 1,
				Price = 0
			};
			products.Add(newProduct);

			#endregion

			const string connSTring = "Data Source=example.db; Version=3;";

			using (var conn = new SQLiteConnection(connSTring))
			{
				conn.Open();

				using (var comm = new SQLiteCommand(conn))
				{
					using (var transaction = conn.BeginTransaction())
					{
						comm.CommandText = "INSERT INTO products (manufacturer, name, version, count, price) values(?,?,?,?,?)";

						foreach (var product in products)
						{
							comm.Parameters.AddWithValue("@0", product.Manufacturer);
							comm.Parameters.AddWithValue("@1", product.Name);
							comm.Parameters.AddWithValue("@2", product.Version);
							comm.Parameters.AddWithValue("@3", product.Count);
							comm.Parameters.AddWithValue("@4", product.Price);
							comm.ExecuteNonQuery();
						}

						transaction.Commit();
					}
				}

				conn.Close();

				ProductsListView.ItemsSource = products;
			}
		}

		/// <summary>
		/// Read some value from database
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReadSomeValues(object sender, RoutedEventArgs e)
		{
			ProductsListView.ItemsSource = null;

			const string connSTring = "Data Source=example.db; Version=3;";

			using (var conn = new SQLiteConnection(connSTring))
			{
				conn.Open();

				using (var comm = new SQLiteCommand(conn))
				{
					comm.CommandText = "SELECT rowid, * FROM products";

					using (var reader = comm.ExecuteReader())
					{
						if (reader.HasRows)
						{
							var products = new List<Product>();

							while (reader.Read())
							{
								var newProduct = new Product()
								{
									RowId = (long)reader["rowid"],
									Manufacturer = (string)reader["manufacturer"],
									Name = (string)reader["name"],
									Version = (long)reader["version"],
									Count = (long)reader["count"],
									Price = (long)reader["price"]
								};
								products.Add(newProduct);
							}

							ProductsListView.ItemsSource = products;
						}
						else
						{
							MessageBox.Show("There is no result on products table", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
					}
				}
			}
		}

		/// <summary>
		/// Update item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UpdateItem(object sender, RoutedEventArgs e)
		{
			if (ProductsListView.SelectedItem == null)
			{
				return;
			}

			const string connSTring = "Data Source=example.db; Version=3;";

			using (var conn = new SQLiteConnection(connSTring))
			{
				conn.Open();

				using (var commup = new SQLiteCommand(conn))
				{
					commup.CommandText = "UPDATE products SET price='0' WHERE rowid='" + ((Product)ProductsListView.SelectedItem).RowId + "'";

					var rows = commup.ExecuteNonQuery();

					if (rows > 0)
					{
						ReadSomeValues(sender, e);
					}
				}

				conn.Close();
			}
		}

		/// <summary>
		/// Delete item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteItem(object sender, RoutedEventArgs e)
		{
			if (ProductsListView.SelectedItem == null)
			{
				return;
			}

			const string connSTring = "Data Source=example.db; Version=3;";

			using (var conn = new SQLiteConnection(connSTring))
			{
				conn.Open();

				using (var commup = new SQLiteCommand(conn))
				{
					commup.CommandText = "DELETE FROM products WHERE rowid='" + ((Product)ProductsListView.SelectedItem).RowId + "'";

					var rows = commup.ExecuteNonQuery();

					if (rows > 0)
					{
						ReadSomeValues(sender, e);
					}
				}

				conn.Close();
			}
		}

		/// <summary>
		/// Search item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SearchItem(object sender, RoutedEventArgs e)
		{
			ProductsListView.ItemsSource = null;

			const string connSTring = "Data Source=example.db; Version=3;";

			using (var conn = new SQLiteConnection(connSTring))
			{
				conn.Open();

				using (var comm = new SQLiteCommand(conn))
				{
					comm.CommandText = "SELECT rowid, * FROM products WHERE manufacturer LIKE @manufacturer";
					comm.Parameters.AddWithValue("@manufacturer", "Windows");

					using (var reader = comm.ExecuteReader())
					{
						if (reader.HasRows)
						{
							var products = new List<Product>();

							while (reader.Read())
							{
								var newProduct = new Product()
								{
									RowId = (long)reader["rowid"],
									Manufacturer = (string)reader["manufacturer"],
									Name = (string)reader["name"],
									Version = (long)reader["version"],
									Count = (long)reader["count"],
									Price = (long)reader["price"]
								};
								products.Add(newProduct);
							}

							ProductsListView.ItemsSource = products;
						}
						else
						{
							MessageBox.Show("There is no result on products table", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Products Class
	/// </summary>
	public class Product
	{
		public long RowId { get; set; }
		public string Manufacturer { get; set; }
		public string Name { get; set; }
		public long Version { get; set; }
		public long Count { get; set; }
		public long Price { get; set; }
	}
}
