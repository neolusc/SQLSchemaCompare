/**
 * Contains various utility methods
 */
class Utility {
    /**
     * Encode the HTML tags inside the string
     * @param s - The string to encode
     * @returns The string properly encoded
     */
    public static EncodeHtmlEntities(s: string): string {
        return $("<div/>").text(s).html();
    }

    /**
     * Decode the HTML tags inside the string
     * @param s - The string to decode
     * @returns The decoded string
     */
    public static DecodeHtmlEntities(s: string): string {
        return $("<div/>").html(s).text();
    }

    /**
     * Indicates whether the specified string is null or an Empty string.
     * @param s - The string to test.
     */
    public static IsNullOrEmpty(s: string): boolean {
        return s === null || typeof s === "undefined" || s.length < 1;
    }

    /**
     * Indicates whether a specified string is null, empty, or consists only of white-space characters.
     * @param s - The string to test.
     */
    public static IsNullOrWhitespace(s: string): boolean {
        return this.IsNullOrEmpty(s) || s.trim().length < 1;
    }

    /**
     * Perform an ajax call and open the modal dialog filled with the response
     * @param url - The URL of the ajax call
     * @param method - The method (GET/POST)
     * @param data? - The object data to send when the method is POST
     */
    public static OpenModalDialog(url: string, method: string, data?: object): void {
        this.AjaxCall(url, method, data, (result: string): void => {
            $("#myModalBody").html(result);
            $("#myModal").modal("show");
        });
    }

    /**
     * Close the modal dialog
     */
    public static CloseModalDialog(): void {
        $("#myModal").modal("hide");
    }

    /**
     * Parse all the input elements in JSON format
     * @param element - The container to search for input elements
     * @returns The serialized JSON object
     */
    public static SerializeJSON(element: JQuery): object {
        // Ref: https://github.com/marioizquierdo/jquery.serializeJSON#options
        const settings: SerializeJSONSettings = {
            useIntKeysAsArrayIndex: true,
        };

        // Wrap content with a form
        const form: JQuery = element.wrapInner("<form></form>").find("> form");

        // Serialize inputs
        const result: object = <object>element.find("> form").serializeJSON(settings);

        // Eliminate newly created form
        form.contents().unwrap();

        return result;
    }

    /**
     * Perform an ajax call
     * @param url - The URL of the ajax call
     * @param method - The method (GET/POST)
     * @param data - The object data to send when the method is POST
     * @param successCallback - The callback function in case of success
     */
    public static AjaxCall(url: string, method: string, data: object, successCallback: JQuery.Ajax.SuccessCallback<object>): void {
        $.ajax(url, {
            type: method,
            beforeSend: (xhr: JQuery.jqXHR): void => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $("input:hidden[name='__RequestVerificationToken']").val().toString());
            },
            contentType: "application/json",
            data: data !== undefined ? JSON.stringify(data) : "",
            cache: false,
            success: successCallback,
            error: (error: JQuery.jqXHR): void => {
                $("#myModalBody").html(error.responseText);
                $("#myModal").modal("show");
            },
        });
    }
}
