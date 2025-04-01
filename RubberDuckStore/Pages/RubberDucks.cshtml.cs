using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;

namespace RubberDuckStore.Pages {
    public class RubberDucksModel : PageModel {
        //Property that stores the selected duck ID from form submissions
        [BindProperty]
        public int SelectedDuckId { get; set; }

        //List that will hold all the ducks for the dropdown selection
        public List<SelectListItem> DuckList { get; set;}

        // Property that will store the currently selected duck object
        public Duck SelectedDuck { get; set; }

        // Handles HTTP GET requests to the page - loads the list of ducks
        public void OnGet(){
            LoadDuckList();
        } //end method

        // Handles HTTP POST requests (when the user selects a duck) 
        //  load the duck list and gets the deatils for the selected duck
        public IActionResult OnPost(){
            LoadDuckList();
            if (SelectedDuckId != 0){
                SelectedDuck = GetDuckById(SelectedDuckId);
            } //end if 
            return Page();
        }//end method
  

    //Helper method that loads the list of ducks from the SQLite database
    // for displaying the drop-down menu
    private void LoadDuckList(){
        //Create a new list to hold ducks
        DuckList = new List<SelectListItem>();

        //Create a database connection
        using (var connection = new SqliteConnection("DataSource=RubberDucks.db"))
            {
                //Open the connection and execute the SQL command to retrieve the list of ducks
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name FROM Ducks";

                //Use the Reader object to iterate through the resultset (list of ducks)
                //and populate the drop-down menu
                using (var reader = command.ExecuteReader()) {
                    // While there are more records in the resultset, keep looping
                    while (reader.Read()) {
                        //Add the current Duck record to the drop-down menu
                        DuckList.Add(new SelectListItem{
                            Value = reader.GetInt32(0).ToString(), // sets the Duck ID as the value
                            Text = reader.GetString(1) //Duck name as the display text
                        }); //end add duck
                    } //end while
                } //end using
            } //end using
    } //end method

    // Helper method that retrieves a specific duck by its ID from the database
    // Returns all the details about the duck
    private Duck GetDuckById(int id){
        //Create a database connection 
        using (var connection = new SqliteConnection("DataSource=RubberDucks.db")) {
            //Open a new connection and create a command to retrieve a duck from the database
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Ducks WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id); //Using a parameterized query for security 

            //Create a new Reader to read the records in the resultset
            using (var reader = command.ExecuteReader()){
                //There should be only one duck
                if (reader.Read()){
                    return new Duck {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        Price = reader.GetDecimal(3),
                        ImageFileName = reader.GetString(4)
                    };
                } //end if
            }//end using reader
        } //end using connection

        //Return null if there is no matching duck
        return null;

    } //end method
   } //end class

    //Simple model class representing a rubber duck product
    public class Duck{
        //Attributes for the duck class
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageFileName {get; set;}
    }//end duck class

} //end namespace