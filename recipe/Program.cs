using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
/*
API Routes:
Get      /recepies          Get a list of recipes +
Delete   /recepies/{id}     deletes a recipe with the id
Post    /recepies           Adds a new recipie
Get     /recepies/title/{title}   Get a list of recipes filtered by title
Get     /recepies/ingredients/{ingredients}
PUT     /recepies/{id}
*/

var nextRecipeId = 0;
var recipeDict = new ConcurrentDictionary<int, Recipe>();
var newDict = new ConcurrentDictionary<int,Recipe>();

//GET /recepies
app.MapGet("/recepies", () => recipeDict.Values);

//GET /recepies/{id}
app.MapGet("/recepies/{id}", (int id) =>
{
    if (recipeDict.TryGetValue(id, out Recipe? recipe))
    {
        return Results.Ok(recipe);
    }
    else
    {
        return Results.NotFound();
    }
});

//POST /recepies
app.MapPost("/recepies", (CreateOrUpdateDto newRecipe) =>
{
    var newId = Interlocked.Increment(ref nextRecipeId);
    var RecipeToAdd = new Recipe
    {
        id = newId,
        Title = newRecipe.Title,
        Description = newRecipe.Description,
        ImageLink = newRecipe.ImageLink,
        Ingredients = newRecipe.Ingredients
    };

    if (!recipeDict.TryAdd(newId, RecipeToAdd))
    {
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
    return Results.Created($"/recepies/{newId}", RecipeToAdd);

});

// DELETE /recepies/{id}

app.MapDelete("/recepies/{id}", (int id) =>
{

    if (!recipeDict.TryRemove(id, out var _))
    {
        return Results.NotFound();
    }
    return Results.NoContent();

});

//GET recepies/title/{title}
app.MapGet("recepies/title/{title}", (string title) =>
{
    
    var recipeResult = new List<Recipe>();
    foreach (var recipe in recipeDict.Values){
        if(recipe.Title.ToLower().Contains(title.ToLower())||recipe.Description.ToLower().Contains(title.ToLower())){
            recipeResult.Add(recipe);
        }
    }
    return recipeResult;

});

//GET recepies/ingredients/{title}
app.MapGet("recepies/ingredients/{ingredients}", (string ingredients) =>
{
    
    var recipeResult = new List<Recipe>();
    foreach (var recipe in recipeDict.Values){
        foreach (var ingrident in recipe.Ingredients){
            if(ingrident.Name==ingredients&& !recipeResult.Contains(recipe)){
            recipeResult.Add(recipe);
        }

        }
        
    }
    return recipeResult;

});

//PUT
app.MapPut("/recepies/{id}", (int id, CreateOrUpdateDto updateRecipe) =>
 {
     if (!recipeDict.TryGetValue(id, out Recipe? recipe))
     {
         return Results.NotFound();
     }
     recipe.Title= updateRecipe.Title;
     recipe.Description=updateRecipe.Description;
     recipe.ImageLink=updateRecipe.ImageLink;
     recipe.Ingredients=updateRecipe.Ingredients;

     return Results.Ok(recipe);
 });

app.Run();

class Recipe
{
    public int id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageLink { get; set; } = "";
    public List<Ingredients> Ingredients { get; set; } = new List<Ingredients>();
}
class Ingredients
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public string UnitOfMesuare { get; set; } = "";

}
record CreateOrUpdateDto(string Title, string Description, string ImageLink, List<Ingredients> Ingredients);