# Razor Components Testing Library
Testing library for Razor Components, that allows you to easily define your component under test and the expected output HTML
in a `.razor` file. It will automatically compare the input with the expected output using the 
[XMLDiff](https://www.xmlunit.org/) library and pretty print error messages.

The library is currently tied to [xUnit](https://xunit.net/), but it is on the TODO list to make it compatible with all
.NET Core testing libraries.

### Help and input wanted
This is still early days for the library and nothing is set in stone with regards to syntax and functionality.
If you have an idea, suggestion, or bug, please add an [issue](issues) or ping me on [Gitter](https://gitter.im/razor-components-testing-library/community). Pull-requests are also very welcome.

[![Gitter](https://badges.gitter.im/razor-components-testing-library/community.svg)](https://gitter.im/razor-components-testing-library/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Getting started
1. Create the necessary projects (Razor Class Library and xUnit class Library). See the [sample project](master/sample) for an example.
1. Install the [Razor.Components.Testing.Library](https://www.nuget.org/packages/Razor.Components.Testing.Library) library from Nuget into your xUnit test project.
2. Optionally, add an `_Imports.razor` to test project to avoid typing using and inherits statements in each test file.
3. Write `.razor`-based tests.

### Example \_Imports.razor
```cshtml
@inherits Egil.RazorComponents.Testing.RazorComponentTest

@using Microsoft.Extensions.DependencyInjection
@using Microsoft.JSInterop
@using Xunit
@using Moq
@using Egil.RazorComponents.Testing
```

## Example
The test examples below tests the Bootstrap [`Alert.razor`](sample/ComponentLib/Alert.razor) sample component, using the [`AlertTests.razor`](sample/RazorComponentLibTests/AlertTests.razor) test component, both found under the sample folder.

### Component under test - Alert.razor
```cshtml
@code {
    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Dismissable { get; set; } = false;

    [Parameter]
    public string Role { get; set; } = "alert";

    [Parameter]
    public bool Visible { get; set; } = true;

    public void Dismiss()
    {
        // Do something with JsRuntime
    }

    private string CssClass
    {
        get
        {
            var cssClass = "alert fade";

            if (Visible)
                cssClass = $"{cssClass} show";

            if (Color != null)
                cssClass = $"{cssClass} alert-{Color}";

            if (Dismissable)
                cssClass = $"{cssClass} alert-dismissible";

            if (Class != null)
                cssClass = $"{cssClass} {Class}";

            return cssClass;
        }
    }
}
<div class=@CssClass role=@Role>
    @if (Visible)
    {
        @if (ChildContent != null)
        {
            @ChildContent
        }
        @if (Dismissable)
        {
            <button type="button" class="close" aria-label="Close">
                <span aria-hidden="true">&amp;times;</span>
            </button>
        }
    }
</div>
```

### Simple tests
To perform a simple comparison test between the HTML generated by the `<Alert />` component, add a `<Fact />` component to your `.razor`-test-file, include the `<TestSetup>` component, that should contain the setup of the component under test, and an `<ExpectedHtml/>` component that contains the expected output.

E.g.:

```cshtml
<Fact DisplayName="Alert renders as empty without child content">
    <TestSetup>
        <Alert />
    </TestSetup>
    <ExpectedHtml>
        <div class="alert fade show" role="alert"></div>
    </ExpectedHtml>
</Fact>

<Fact DisplayName="Alert renders with child content if provided">
    <TestSetup>
        <Alert>FOO BAR BAZ</Alert>
    </TestSetup>
    <ExpectedHtml>
        <div class="alert fade show" role="alert">FOO BAR BAZ</div>
    </ExpectedHtml>
</Fact>

<Fact DisplayName="Alert adds color when specified">
    <TestSetup>
        <Alert Color="primary" />
    </TestSetup>
    <ExpectedHtml>
        <div class="alert fade show alert-primary" role="alert"></div>
    </ExpectedHtml>
</Fact>

<Fact DisplayName="Providing a role overrides default role value">
    <TestSetup>
        <Alert Role="banner" />
    </TestSetup>
    <ExpectedHtml>
        <div class="alert fade show" role="banner"></div>
    </ExpectedHtml>
</Fact>

<Fact DisplayName="Setting Dismisasable to true renderes dismiss button below child content">
    <TestSetup>
        <Alert Dismissable>
            <strong>Holy guacamole!</strong> You should check in on some of those fields below.
        </Alert>
    </TestSetup>
    <ExpectedHtml>
        <div class="alert fade show alert-dismissible" role="alert">
            <strong>Holy guacamole!</strong> You should check in on some of those fields below.
            <button type="button" class="close" aria-label="Close">
                <span aria-hidden="true">&amp;times;</span>
            </button>
        </div>
    </ExpectedHtml>
</Fact>
```

**NOTE:** In the future, the value in the 'DisplayName' attribute will be displayed by the test-runner.

### Custom tests
If you want more control over the assertion, use a `<HtmlSnippet/>` component instead of a
`<ExpectedHtml/>` component. Then the default unit test will not run, and you can add your
own, where you can access the `RenderResults` property and do any comparison of the HTML (XML)
you want.

```cshtml
@code {
    [Fact]
    public void A_Custom_Test()
    {
        // assert
        var result = RenderResults.Single(x => x.Id == (nameof(A_Custom_Test)));
        result.RenderedHtml.ShouldBe(result.Snippets.First());
    }
}
<Fact Id=@nameof(A_Custom_Test)>
    <TestSetup>
        <Alert Role="banner" Color="secondary" class="my-custom-class" Dismissable>
            <strong>Holy guacamole!</strong> You should check in on some of those fields below.
        </Alert>
    </TestSetup>
    <HtmlSnippet>
        <div class="alert fade show alert-secondary alert-dismissible my-custom-class" role="banner">
            <strong>Holy guacamole!</strong> You should check in on some of those fields below.
            <button type="button" class="close" aria-label="Close">
                <span aria-hidden="true">&amp;times;</span>
            </button>
        </div>
    </HtmlSnippet>
</Fact>
```

If you want to assert directly on the rendered component or change its parameters, use the component reference syntax (`@ref`). The `Render` method allows you to trigger a synchronous re-render manually as needed.

```cshtml
@code {
    bool isVisible = true;

    [Fact(DisplayName = "When Visible is toggled to false, all child content is removed from alert")]
    public void DismissTest()
    {
        // initial assert
        var result = RenderResults.Single(x => x.Id == (nameof(DismissTest)));
        result.RenderedHtml.ShouldBe(result.Snippets[0]);
        //Assert.Equal(true, sut.Visible); BUG - sut field not generated: https://github.com/aspnet/AspNetCore/issues/13099

        // act
        isVisible = false;
        this.Render();

        // dismiss assert
        var dismissResult = RenderResults.Single(x => x.Id == (nameof(DismissTest)));
        dismissResult.RenderedHtml.ShouldBe(result.Snippets[1]);
        //Assert.Equal(false, sut.Visible); BUG - sut field not generated: https://github.com/aspnet/AspNetCore/issues/13099
    }
}
<Fact Id=@nameof(DismissTest)>
    <TestSetup>
        <Alert @ref="sut" Visible=@isVisible>
            <strong>Holy guacamole!</strong> You should check in on some of those fields below.
        </Alert>
    </TestSetup>
    <HtmlSnippet>
        <div class="alert fade show" role="alert">
            <strong>Holy guacamole!</strong> You should check in on some of those fields below.
        </div>
    </HtmlSnippet>
    <HtmlSnippet>
        <div class="alert fade" role="alert"></div>
    </HtmlSnippet>
</Fact>
```

### Injecting services
If the component under test needs access to services, those can be injected by
overriding the `AddServices` method. E.g.:

```cshtml
@code {
    protected override void AddServices(IServiceCollection services)
    {
        services.AddScoped<IJSRuntime>(_ => Mock.Of<IJSRuntime>());
    }
}
```
