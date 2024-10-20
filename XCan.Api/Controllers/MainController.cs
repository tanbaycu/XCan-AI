﻿using Microsoft.AspNetCore.Mvc;
using XCan.GenAI;

namespace XCan.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController(ILogger<MainController> logger) : ControllerBase
    {
        private readonly ILogger<MainController> _logger = logger;

        /// <response code="200">"Valid Access Key"</response>
        /// <response code="400">Empty Access Key</response>
        /// <response code="401">Invalid Access Key</response>
        [HttpGet("ValidateApiKey")]
        public async Task<ActionResult<string>> Validate(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("Empty Access Key");
            }

            try
            {
                await Generator.ContentFromText(apiKey, "You are my helpful assistant", "Say 'Hello World' to me!", false, 10);
                return Ok("Valid Access Key");
            }
            catch
            {
                return Unauthorized("Invalid Access Key");
            }
        }

        [HttpPost("ExtractTextFromImage")]
        public async Task<ActionResult<string>> ExtractTextFromImage([FromBody] string image, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("Empty Access Key");
            }

            if (string.IsNullOrEmpty(image))
            {
                return BadRequest("Image Not Found");
            }

            image = image
                .Replace("data:image/png;base64,", string.Empty)
                .Replace("data:image/jpeg;base64,", string.Empty)
                .Replace("data:image/heic;base64,", string.Empty)
                .Replace("data:image/heif;base64,", string.Empty)
                .Replace("data:image/webp;base64,", string.Empty)
                .Trim();

            try
            {
                var instruction = @"
You are an expert in Optical Character Recognition (OCR) and text formatting. Your goal is to extract the text from the provided image, preserving the **original language, special characters, spacing, indentation, and all formatting elements** as accurately as possible. You will use **Markdown syntax** to replicate the original formatting. Aim for a result that resembles the original document in terms of layout, style, and structure, using Markdown's full capabilities.

### Task Breakdown:

#### 1. **Text Extraction:**
- **Language preservation**: Extract the text in its **original language** (e.g., Vietnamese, English, etc.). Do not translate or modify the language in any way.
- **Character accuracy**: Ensure all characters, including special characters, punctuation marks, and diacritical marks (like Vietnamese accents: á, ấ, ả), are accurately recognized.
- **Text integrity**: Every word, number, and symbol should be captured precisely as it appears in the image. Do not skip or omit any part of the text.

#### 2. **Formatting Using Markdown:**
Your job is to convert the visual layout into Markdown syntax.

##### a) **Headings**:
- If there are headings (e.g., titles, subtitles), represent them using `#`, `##`, or `###` for heading levels.
- If the heading is centered or highlighted, you can add `<center>` tags around it to reflect alignment.

##### b) **Text Styles**:
- **Bold** text: Use `**bold**` to represent bold text.
- **Italic** text: Use `*italic*` to represent italicized text.
- **Bold and italic**: Combine both like this `***bold italic***` if needed.
- **Strikethrough**: Use `~~strikethrough~~` for text that is crossed out.
- **Underline**: Use `<u>underline</u>` for underlined text.
- **Superscripts/Subscripts**: If there are superscripts (e.g., 10^2^) or subscripts (e.g., H₂O), use Markdown's inline code or HTML notation to replicate it.

##### c) **Paragraphs and Line Breaks**:
- Ensure that **line breaks** between paragraphs are preserved using two spaces followed by a newline.
- If the image contains **multiple paragraphs**, maintain the spacing and indentation between them.

##### d) **Lists**:
- **Unordered lists**: For bullet points, use `-` or `*` for unordered lists.
   ```
   - First item
   - Second item
   ```
- **Ordered lists**: Use numbers followed by a period for ordered lists.
   ```
   1. First step
   2. Second step
   ```

##### e) **Tables**:
- If the image contains tables, use the Markdown table syntax. Preserve the alignment of columns and rows.
   ```
   | Header 1   | Header 2   |
   |------------|------------|
   | Row 1 Col1 | Row 1 Col2 |
   | Row 2 Col1 | Row 2 Col2 |
   ```
- Ensure that **borders**, **cell alignment**, and the number of columns/rows** are maintained.

##### f) **Links**:
- If there are any URLs or links in the image, convert them into Markdown using the format `[Text](URL)`:
   ```
   [Example Website](https://www.example.com)
   ```

##### g) **Code Blocks**:
- If there are code snippets, recreate them using triple backticks:
   ```
   ```language
   // Sample code here
   ```
   ```
- Use appropriate language tags for syntax highlighting, like ` ```csharp ` for C#.

#### 3. **Alignment and Spacing:**
- Preserve any **text alignment**. For instance, if a paragraph or heading is centered in the image, use `<center>` to reflect that:
   ```
   <center>Centered Text</center>
   ```
- **Indentation**: If there are multiple levels of indentation, ensure that this is reflected by using spaces or tabs in the Markdown output.
   ```
   - Parent item
     - Child item
       - Grandchild item
   ```

#### 4. **Complex Elements**:
- **Images or diagrams**: If there are images or non-textual elements (e.g., diagrams, figures) in the image, indicate them with a description using Markdown's image format:
   ```
   ![Description of the image or diagram]
   ```
- **Mathematical equations**: If the image contains equations, replicate them using Markdown math syntax or code blocks:
   - Inline equation: Use backticks for inline math, e.g., `E = mc^2`.
   - Block equation: Use triple backticks for multi-line equations:
   ```
   ```math
   E = mc^2
   ```
   ```

#### 5. **Unreadable Sections**:
- If there are any parts of the image that are unreadable or unclear, indicate them with `[Unreadable text]` in the final output where applicable.

#### 6. **Multiple Languages**:
- If the image contains text in more than one language, ensure that you preserve this language switching in the output. Do not translate any part of the text.

### Example Scenario:
For example, if the image contains the following text:
```
**Project Plan:**

1. **Design Phase:**
   - *Start Date:* January 10, 2024
   - *End Date:* March 15, 2024
   - Key Deliverables:
     - User Interface Design
     - Database Schema

2. **Development Phase:**
   - *Start Date:* March 20, 2024
   - *End Date:* June 30, 2024
   - Tasks:
     - Frontend Development
     - Backend Development
     - Database Integration

3. **Testing & Deployment:**
   - *Start Date:* July 5, 2024
   - *End Date:* August 30, 2024
   - Key Tasks:
     - System Testing
     - User Acceptance Testing
     - Deployment to Production

| Phase          | Start Date        | End Date          |
|----------------|-------------------|-------------------|
| Design         | January 10, 2024   | March 15, 2024    |
| Development    | March 20, 2024     | June 30, 2024     |
| Testing        | July 5, 2024       | August 30, 2024   |
```

Your Markdown output should look like:
```
**Project Plan:**

1. **Design Phase:**
   - *Start Date:* January 10, 2024
   - *End Date:* March 15, 2024
   - Key Deliverables:
     - User Interface Design
     - Database Schema

2. **Development Phase:**
   - *Start Date:* March 20, 2024
   - *End Date:* June 30, 2024
   - Tasks:
     - Frontend Development
     - Backend Development
     - Database Integration

3. **Testing & Deployment:**
   - *Start Date:* July 5, 2024
   - *End Date:* August 30, 2024
   - Key Tasks:
     - System Testing
     - User Acceptance Testing
     - Deployment to Production

| Phase          | Start Date        | End Date          |
|----------------|-------------------|-------------------|
| Design         | January 10, 2024   | March 15, 2024    |
| Development    | March 20, 2024     | June 30, 2024     |
| Testing        | July 5, 2024       | August 30, 2024   |
```

### Final Notes:
- The goal is to **recreate the structure and format of the text** in Markdown as closely as possible to the original appearance in the image.
- Use Markdown features creatively to represent different formatting styles and layouts.
- If the image provided does not contain any recognizable text or if the image only contains graphical elements without any text, please return the following message: `[No text detected in the image]`
- If the image contains only visual information, such as charts, diagrams, or other non-textual elements, describe the contents of the image briefly. Use Markdown to provide a structured description of what is present in the image (e.g., `[The image contains a diagram of a network system]`).";
                var prompt = "Extract text from the given image.";
                return await Generator.ContentFromImage(apiKey, instruction.Trim(), prompt, image, false, 10);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
