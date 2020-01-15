import React from 'react'
import * as signalR from '@aspnet/signalr';

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

export default function ProjectItem(props) {

    const [connection, setConnection] = React.useState(null);
    const [progress, setProgress] = React.useState('');
    
    function startWatching() {
        var con = new signalR.HubConnectionBuilder()
            .withUrl('/Hubs/Mapping').build();
        con.on('Progress', (current, total) => {
            setProgress(current.toString() + ' / ' + total.toString());
        });
        con.start()
            .then(_ => con.invoke('StartWatching', props.name))
            .catch(e => console.error(e));
        setConnection(con);
    }

    function stopWatching() {
        connection.stop();
        setConnection(null);
    }

    function startMapping() {
        fetch('api/Mapping/Start/' + props.name, {
            method: 'POST'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            startWatching();
        })
        .catch((e) => {
            console.error(e);
        });
    }

    return (
        <li style={styles.li}>
            <span>
                {props.name}
            </span>
            <span>
                {progress}
            </span>
            <button onClick={() => startMapping()}>
                &times;
            </button>
            <button className='rm' onClick={() => props.removeProject(props.name)}>
                &times;
            </button>
        </li>
    )
}
