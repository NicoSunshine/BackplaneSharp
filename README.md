The class is a port of "Backplane.php", presented here: http://wiki.aboutecho.com/w/page/28068607/Single%20Sign%20On

Is designed to create a simple wrap to the Backplane requests

Sample usage:

````C#
  var echoBackplane = new Backplane("Backplane bus name", "Backplane password");
  
  echoBackplane.Login(baseUrl, username, userAvatar);
  
  //...
  
  echoBackplane.Logout(baseUrl, username);
````

Or, if you want more control in each JSON parameter:

````C#
  echoBackplane.Login(new BackplaneJSON() {
      Source = baseUrl,
      Type = "identity/logout",
      Context = baseUrl,
      Id = baseUrl + "/" + username,
      DisplayName = username,
      IdentityUrl = baseUrl + "/" + username,
      Username = username        
  });


  echoBackplane.Logout(new BackplaneJSON() {
      Source = baseUrl,
      Id = baseUrl + "/" + username,
      IdentityUrl = baseUrl + "/" + username
  });
````

Lastly, you could use the request if you want to pass a custom data to the POST:

````C#
  echoBackplane.SendRequest("JSON");
````
