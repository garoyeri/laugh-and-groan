import React, { useState } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Alert, Form, Button } from "react-bootstrap";
import { useFormFields } from "../hooks";
import { useApiCalls } from "../hooks";

const PostCreator = () => {
  const { isAuthenticated } = useAuth0();
  const [loading, setLoading] = useState(false);
  const [postStatus, setPostStatus] = useState({
    completed: false,
    message: "",
  });
  const [fields, handleFieldsChange] = useFormFields({
    linkTitle: "",
    linkUrl: "",
  });
  const { createPost } = useApiCalls();

  async function handleSubmit(event) {
    event.preventDefault();

    if (!isAuthenticated) return;

    try {
      setLoading(true);
      setPostStatus({ completed: false, message: "" });
      await createPost({
        url: fields.linkUrl,
        title: fields.linkTitle,
      });

      setLoading(false);
      setPostStatus({
        completed: true,
        message: "",
      });
    } catch (error) {
      setLoading(false);
      setPostStatus({
        completed: true,
        message: error.message || "An unknown error occurred!",
      });
    }
  }

  return (
    <div>
      <Form inline onSubmit={handleSubmit}>
        <Form.Group controlId="linkTitle" className="mr-2">
          <Form.Label>Title</Form.Label>
          <Form.Control
            type="text"
            value={fields.linkTitle}
            onChange={handleFieldsChange}
          />
        </Form.Group>
        <Form.Group controlId="linkUrl" className="mr-2">
          <Form.Label>Url</Form.Label>
          <Form.Control
            type="url"
            value={fields.linkUrl}
            onChange={handleFieldsChange}
          />
        </Form.Group>
        <Button type="submit" disabled={loading || !isAuthenticated}>
          Post Link
        </Button>
      </Form>
      {postStatus.completed && postStatus.message && (
        <Alert className="mt-2" variant="danger">
          {postStatus.message}
        </Alert>      
      )}
      {postStatus.completed && !postStatus.message && (
        <Alert className="mt-2" variant="success">
          Link was posted successfully!
        </Alert>      
      )}

    </div>
  );
};

export default PostCreator;
