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
    const [mounted, setMounted] = React.useState(false);
    
    function startWatching() {
        var con = new signalR.HubConnectionBuilder()
            .withUrl('/Hubs/Mapping').build();
        con.on('Progress', (current, total) => {
            if (mounted) {
                setProgress(current.toString() + ' / ' + total.toString());
            }
        });
        con.start()
            .then(_ => con.invoke('StartWatching', props.name))
            .catch(e => console.error(e));
        if (mounted) {
            setConnection(con);
        }
        else {
            con.stop();
        }
    }

    function stopWatching() {
        if (connection !== null) {
            connection.stop();
            if (mounted) {
                setConnection(null);
            }
        }
    }

    function startMapping() {
        fetch('api/Mapping/Start/' + props.name, {
            method: 'PUT'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            startWatching();
        })
        .catch((e) => {
            console.error(e);
        });
    }

    function stopMapping() {
        fetch('api/Mapping/Stop/' + props.name, {
            method: 'PUT'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            stopWatching(false);
        })
        .catch((e) => {
            console.error(e);
        });
    }

    function switchMapping() {
        connection === null
            ? startMapping()
            : stopMapping();
    }

    function mappingBtnTxt() {
        return connection === null
            ? "Start mapping..."
            : "Stop mapping";
    }

    function removeProject() {
        stopWatching();
        props.removeProject(props.name);
    }

    React.useEffect(() => {
        setMounted(true);
        return () => {
            setMounted(false);
            stopWatching();
        }
    }, []);

    return (
        <li style={styles.li}>
            <span>
                {props.name}
            </span>
            <span>
                {progress}
            </span>
            <button
                type="button"
                className="btn btn-primary btn-sm"
                onClick={() => switchMapping()}>{mappingBtnTxt()}</button>
            <button
                type="button"
                className="btn btn-danger btn-sm"
                onClick={() => removeProject()}>Remove</button>
        </li>
    )
}
