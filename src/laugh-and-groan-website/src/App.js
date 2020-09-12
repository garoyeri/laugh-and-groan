import React, { useState, useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { Container, Row, Col, Jumbotron } from "react-bootstrap";
import "./App.css";
import { LoginButton, LogoutButton, PostList, PostCreator } from "./components";
import { useApiCalls } from "./hooks";

function App() {
  const { user, isAuthenticated } = useAuth0();
  const [profileData, setProfileData] = useState({});
  const { getMe } = useApiCalls();

  useEffect(() => {
    if (isAuthenticated) {
      getMe().then((data) => setProfileData(data));
    }
  }, [isAuthenticated]);

  return (
    <div className="App">
      <Container>
        <Row>
          <Col className="mb-2">
            {!isAuthenticated && <LoginButton />}
            {isAuthenticated && (
              <>
                <LogoutButton />
                <span className="ml-2">
                  {user.name} : {profileData.userName}
                </span>
              </>
            )}
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
            <PostCreator />
          </Col>
        </Row>
        <Row><Col><hr /></Col></Row>
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
