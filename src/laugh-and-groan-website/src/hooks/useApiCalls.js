import { useAuth0 } from "@auth0/auth0-react";

const apiDomain = "https://api.laughandgroan.com";

const useApiCalls = () => {
  const { getAccessTokenSilently, isAuthenticated } = useAuth0();

  const getPosts = async () => {
    if (!isAuthenticated) return [];

    const accessToken = await getAccessTokenSilently();
    const url = "/posts";

    const response = await fetch(apiDomain + url, {
      headers: {
        Authorization: `${accessToken}`,
      },
    });
    
    const data = await response.json();

    if (!response.ok) {
      throw data;
    }

    return data?.data || [];
  };

  const getMe = async () => {
    if (!isAuthenticated) return {};

    const accessToken = await getAccessTokenSilently();
    const url = "/users/me";

    const response = await fetch(apiDomain + url, {
      headers: {
        Authorization: `${accessToken}`,
      },
    });

    const data = await response.json();

    if (!response.ok) {
      throw data;
    }

    return data;
  };

  const createPost = async (post) => {
    if (!isAuthenticated) return {};

    const accessToken = await getAccessTokenSilently();
    const url = "/posts";

    const response = await fetch(apiDomain + url, {
      headers: {
        Authorization: `${accessToken}`,
        "Content-Type": "application/json",
      },
      method: "POST",
      body: JSON.stringify(post),
    });

    const data = await response.json();

    if (!response.ok) {
      throw data;
    }

    return data;
  };

  return { getPosts, getMe, createPost };
};

export default useApiCalls;
