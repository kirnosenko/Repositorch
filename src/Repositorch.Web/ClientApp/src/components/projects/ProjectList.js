import React from 'react'
import { Redirect } from 'react-router-dom'
import ContentToLoad from '../ContentToLoad'
import ProjectItem from './ProjectItem'

export default function ProjectList() {

    const [list, setList] = React.useState(null);
    const styles = {
        ul: {
            listStyle: 'none',
            margin: 0,
            padding: 0
        }
    }

    function removeProject(name) {
        fetch("api/Projects/Remove/".concat(name), {
            method: 'DELETE'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            setList(list.filter((x) => x !== name));
        })
        .catch((error) => {
            console.error(error);
        });
    }

    function renderProjectList(data) {
        if (data.length === 0) {
            return <Redirect to='/new' />
        }

        return (
            <ul style={styles.ul}>
                {data.map(projectName => {
                    return (
                        <ProjectItem
                            name={projectName}
                            key={projectName}
                            removeProject={removeProject}
                        />
                    )
                })}
            </ul>
        )
    }

    return (
        <ContentToLoad
                url="api/Projects/GetNames"
                renderData={renderProjectList}
                data={list}
                setData={setList} />
    );
}
