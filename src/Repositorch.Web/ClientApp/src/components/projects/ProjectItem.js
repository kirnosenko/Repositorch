import React from 'react'

const styles = {
    li: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '.5rem 1rem',
        border: '1px solid #ccc',
        borderRadius: '4px',
        marginBottom: '.5rem'
    }
}

function ProjectItem({ name }) {
    return (
        <li style={styles.li}>
            <span>
                {name}
            </span>
            <button className='rm'>
                &times;
      </button>
        </li>
    )
}

export default ProjectItem
