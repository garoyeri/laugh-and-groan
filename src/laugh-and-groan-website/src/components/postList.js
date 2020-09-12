import React, { useState, useEffect } from "react";
import { CardColumns } from "react-bootstrap";
import { useAuth0 } from "@auth0/auth0-react";
import { PostItem } from ".";
import { useApiCalls } from "../hooks";

const PostList = () => {
  const { isAuthenticated } = useAuth0();
  const [posts, setPosts] = useState([]);
  const { getPosts } = useApiCalls();

  useEffect(() => {
    if (isAuthenticated) {
      getPosts().then((data) => setPosts(data || []));
    }
  }, [isAuthenticated]);

  return (
    <CardColumns>
      {posts.map((p) => {
        return <PostItem {...p} key={p.id} />;
      })}
    </CardColumns>
  );
};

export default PostList;
