import React, { useState, useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Container, Row, Col, Jumbotron } from "react-bootstrap";
import "./App.css";
import LoginButton from "./loginButton";
import LogoutButton from "./logoutButton";
import PostList from './postList';

function App() {
  const { user, isAuthenticated, getAccessTokenSilently } = useAuth0();
  const [profileData, setProfileData] = useState({})

  useEffect(() => {
    const getMe = async () => {    
      const apiDomain = "https://api.laughandgroan.com";

      try {
        const accessToken = await getAccessTokenSilently();
        const url = "/users/me";

        const response = await fetch(apiDomain + url, {
          headers: {
            Authorization: `${accessToken}`,
          },
        });

        const data = await response.json();

        setProfileData(data);
      } catch (e) {
        console.log(e);
      }
    }

    if (isAuthenticated) {
      getMe();
    }
  }, [isAuthenticated]);

  return (
    <div className="App">
      <Container>
        <Row>
          <Col className="mb-2">
            {!isAuthenticated && 
              <LoginButton />
            }
            {isAuthenticated &&
              <>
                <LogoutButton />
                <span className="ml-2">{user.name} : {profileData.userName}</span>
              </>
            }
          </Col>
        </Row>
        <Row>
          <Col>
            <Jumbotron>
              <h1>Laugh and Groan</h1>
              <p>The premier site for getting your laughs (and groans).</p>
            </Jumbotron>
          </Col>
        </Row>
        <Row>
          <Col>
            <PostList></PostList>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default App;
