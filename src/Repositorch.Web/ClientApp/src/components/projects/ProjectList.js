import React from 'react'
import { ContentToLoad } from '../ContentToLoad'
import ProjectItem from './ProjectItem'

const styles = {
    ul: {
        listStyle: 'none',
        margin: 0,
        padding: 0
    }
}

function renderProjectList(data) {
    return (
        <ul style={styles.ul}>
            {data.map(projectName => {
                return (
                    <ProjectItem
                        name={projectName}
                        key={projectName}
                    />
                )
            })}
        </ul>
    )
}

export default function ProjectList() {
    return (
        <ContentToLoad url="api/Projects/GetNames" renderData={renderProjectList} />
    );
}
