import React, { Component } from 'react';
import ProjectList from './projects/ProjectList';
import CreateProject from './projects/CreateProject';

function createProject(name)
{
    fetch("api/Projects/Create", {
        method: 'post',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            "Name": name
        })})
        .then((response) => {
            if (response.ok) return response.json();
            throw new Error(response.status);
        })
        .then((data) => {
            console.log(data);
        })
        .catch((error) => {
            console.log(error);
        });
}

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
        <div>
            <CreateProject onCreate={createProject} />
            <ProjectList />
        </div>
    );
  }
}
