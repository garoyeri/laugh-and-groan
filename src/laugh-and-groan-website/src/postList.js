import React, { useState, useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";

const PostList = () => {
  const { getAccessTokenSilently, isAuthenticated } = useAuth0();
  const [posts, setPosts] = useState([]);

  useEffect(() => {
    const getPosts = async () => {
      const apiDomain = "https://api.laughandgroan.com";

      try {
        const accessToken = await getAccessTokenSilently();
        const postsUrl = "/posts";

        const response = await fetch(apiDomain + postsUrl, {
          headers: {
            Authorization: `${accessToken}`,
          },
        });

        const data = await response.json();

        setPosts(data);
      } catch (e) {
        console.log(e);
      }
    };

    getPosts();
  }, []);

  return <div>Posts</div>;
};

export default PostList;
