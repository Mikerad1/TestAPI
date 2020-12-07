1. Change the db connection string in API appsettings
2. Set API as startup project and update database in the package manager console
3. Run both the api and web application

Logic steps
* The privacy page cannot be accessed when the user is not authenticated
1. Press the login button
2. The button sends the username and password admin to the api for authentication
3. The api builds a claim and token and sends it back to the app
4. The app Uses the token and turns it into a claim.
5. The claim is then used for normal auth checks.

############################################

Just go through the logic from button click to accessing the privacy page
and let me know if there are any questions.

###########################################
It's crappy coding but just wanted to build a quick one :D