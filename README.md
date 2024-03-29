<p align="center">
<img width="256" src="https://cdn.discordapp.com/avatars/894305597569257492/d5cb90076cc4e2a2d5ed3c2cc1704649.png?size=256" />
</p>

## Dev guide
### First run
- Navigate to `Huppy.App`
- Execute `dotnet restore`
- Execute `dotnet run`
- Navigate to debug folder and from there copy `appsettings.json` to the root folder 

> `appsettings.json` is generated each time when found missing, when moved to the root project directory (`Huppy.App`), it will be copied each build to debug folder 

### Migration
- Comment out all lines under `// start bot ` in `Program.cs`
- Copy connection string to `HuppyDbContext#OnConfiguring` `optionsBuilder.UseSqlite` from appsettings
- Use `CreateMigration.bat` to create migration with name of it as command argument
- Reverse changes from `Program.cs` & `HuppyDbContext`

## Git guide
### Issue creation
- Open Huppy issues on github [link](https://github.com/HueByte/Huppy/issues)
- Click `New Issue`
- Select right category/template
- Fill the title and task related fields
- Add proper labels 
- Assign to Huppy project in `Projects` (right menu)
- Set priority and size

### Branch creation
There are 2 ways of branch creation
> Github 
<br>(Recommended, When branches are created from issues, their pull requests are automatically linked.)
- Navigate to the issue 
- On the right side menu press `Create a branch` in `Development` section
- Checkout locally to created branch on github

> Locally 
- Checkout to branch (pattern `{issueNumber}-{here-description}`)

### Commits
- Use pattern for tasks `T {#issueNumber} {descriptionTopic1} & {descriptionTopic2}...`
- Use pattern for bugs `B {#issueNumber} {descriptionTopic1} & {descriptionTopic2}...`

### Pull Request
- In title for tasks put `[T or B] {#issueNumber} {description}` (example *T #63 Moved utilities to Utilities folder*)
- In description put `- [*T* or *B*] [*resolves* or *fixes*] {#issueNumber}` (for more information about closing keywords [click here](https://docs.devart.com/studio-for-sql-server/source-controlling-databases/associating-commits-with-github-issues.html))

### Other info
- Example of Task with PR properly linked [Issue #47](https://github.com/HueByte/Huppy/issues/47)
- To display task in github description use (`- #{issueNumber}`)
