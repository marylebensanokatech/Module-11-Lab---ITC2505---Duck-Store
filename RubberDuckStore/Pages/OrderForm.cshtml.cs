using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace RubberDuckStore.Pages
{
    public class OrderFormModel : PageModel {
        [BindProperty]
        public Order? order { get; set; }
        public Duck? duck { get; set; }

        //Implement the OnGet method - overiding from the parent class 
        public IActionResult OnGet(int duckId) {
            // Get the duck that the user selected (duckID is on the get request)
            Duck = GetDuckById(duckId);
            // If there is no duck with that ID, return results of the not found method
            if (Duck == null) {
                return NotFound();
            }//end end if

            //Create a new order
            Order = new Order { DuckId = duckId };

            //Return the page to be displayed in the browser
            return Page();
        }//end onGet

        // OnPost method is called when the user places the order
        public IActionResult OnPost() {
            // Check is the Model state is not valid - if it's not 
            // then get the duck using the duck id and display the page
            if (!ModelState.IsValid){
                //Get the duck and return the page
                Duck = GetDuckById(Order.DuckId);
                return Page();
            }//end if 

            //Save the order
            int orderId = SaveOrder(Order);

            //Return a redirect to the order confirmation page
            //This will send the user to the order confirmation page after
            //they placed the order
            return RedirectToPage("OrderConfirmation", new {orderId = orderId});
        } // end on post method

        // This method looks up the duck ID in the database and creates 
        // a new duck populated with the values for that ID.
        private Duck GetDuckById(int id) {
            //Connect to the database
            using (var connection = new SqliteConnection("Data Source=RubberDucks.db"))
                {
                    //Open the connection and create a command object to execute
                    // SQL statements
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Assign a SELECT statement to the command object
                    command.CommandText = "SELECT * FROM Ducks WHERE Id = @Id";
                    //Set the parameter for the duck id
                    command.Parameters.AddWithValue("@Id", id);

                    //Execute the SQL statement to get the duck data from the database
                    using (var reader = command.ExecuteReader()) {
                        // Read the record in the resultset returned from the db
                        if (reader.Read()) {
                            // Assign the values for the duck we retrieved from the db
                            //to the Duck object the method returns
                            return new Duck {
                                // Assign the values from the db
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                ImageFileName = reader.GetString(4)
                            }; //end new duck
                        } //end if
                    }//end using block
                } //end using block

                //Return null if there was no duck, the connection failed, etc.
                return null;
        }//end method

        //Save Order method - this method opens a connection to the database
        // and saves the order in the orders table
        private int SaveOrder(Order order) {
            // Make a connection to the database
            using (var connection = new SqliteConnection("DataSource=RubberDucks.db")) {
                //Open the connection and create a new command
                connection.Open();
                var command = connection.CreateCommand();

                //Set the text of the command to the insert statement we need to
                //run to insert the order record into the database & select the last row id
                command.CommandText = @"INSERT INTO Orders(DuckId, CustomerName, 
                        Customer Email, Quantity) VALUES (@DuckId, @CustomerName, 
                        @CustomerEmail, @Quantity); SELECT last_insert_rowid();";
                
                //Add the parameters for the values in the order form
                command.Parameters.AddWithValue("@DuckId", order.DuckId);
                command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                command.Parameters.AddWithValue("@CustomerEmail", order.CustomerEmail);
                command.Parameters.AddWithValue("@Quantity", order.Quantity);

                //Return the order row id
                return Convert.ToInt32(command.ExecuteScalar());
            } //end using
        } //end save order method
    }//end class

    //Order class - holds a user's order for a duck
    public class Order {
        //Attributes for the order class (attributes are variables that describe an object)
        public int Id { get; set; }
        public int DuckId { get; set; }
        [Required]
        public string CustomerName {get; set;}
        [Required, EmailAddress]
        public string CustomerEmail {get; set;}
        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set;}
    } //end order class

}//end namespaces