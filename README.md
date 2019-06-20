# Custom IIS Handler

## Install Instruction (IIS Manager)
+ Add Managed Module Mappings into your current site
- Set request path to "*" (Everything)
- Set module to "Cgi Module"
- Select your built executables
- Name it whatever you want
- Configure request restrictions mapping to both file and folder
- Edit Feature Permissions and tick the "Execute"

## Additional notes:- 
+ Make sure you enable CGI first
+ Make sure your custom module is located at the top most in Ordered List view