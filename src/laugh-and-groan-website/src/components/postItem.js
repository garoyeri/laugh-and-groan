import React from "react";
import { Card } from "react-bootstrap";

const PostItem = (props) => {
  const { whenPublished, title, url } = props;

  return (
    <Card className="p-2">
      <Card.Title>{title || "Untitled"}</Card.Title>
      <Card.Text>
        <a href={url} rel="noopener">
          {url}
        </a><br />
        {new Date(Date.parse(whenPublished)).toDateString()}
      </Card.Text>
    </Card>
  );
};

export default PostItem;
