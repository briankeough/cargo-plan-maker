import * as React from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import LinearProgress from '@mui/material/LinearProgress';
import DeleteIcon from '@mui/icons-material/Delete';
import { DataGrid } from '@mui/x-data-grid';
import { apiDomain } from '../config/Config';

export default function DestinationSetup() {

  const gridColumns = [
    { field: 'name', headerName: 'Name', flex: 1 },
    { field: 'maximumWeight', headerName: 'Maximum Pallet Weight', flex: 1 },
    { field: 'actions', headerName: 'Actions', flex: 1, renderCell: (params) => {
      return (
        <IconButton aria-label="delete"
          color='primary'
          onClick={(e) => handleDelete(e, params.row)}
        >
          <DeleteIcon />
        </IconButton>
      );
    }}
  ];

  const [destinationInfo, setDestinationInfo] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [name, setName] = React.useState('');
  const [weight, setWeight] = React.useState('');

  const resetFields = () => {
    setName('');
    setWeight('');
    setLoading(false);
  }
  
  const handleGetDataResponse = (json) => {
    setDestinationInfo(addIdToEachObjectInArray(json));
    setLoading(false);
  }

  const handleDeleteResponse = (json) => {
    let status = json.status;

    if (status === 'success'){
      console.log(`Successfully delete: ${json.id}`)
      resetFields();
      getData();
 
    } else {
      console.log(`Not successfully deleted: ${status}`)
    }
  }

  const handleSubmitResponse = (json) => {
    let status = json.status;

    if (status === 'success'){
      console.log(`Successfully saved: ${json.id}`)
      resetFields();
      getData();
 
    } else {
      console.log(`Not successfully saved: ${status}`)
    }
  }

  const handleDelete = async (e, row) => {
    setLoading(true);

    try {
      await fetch(`${apiDomain}/api/delete-destination-info`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: `{
          "RowKey" : "${row.rowKey}"
        }`
      })
      .then(response => response.json())
      .then(responseData => handleDeleteResponse(responseData))
    
    } catch (error) {
      handleError(error);
    }
  }

  const handleSubmit = async () => {
    setLoading(true);
    
    try {
      await fetch(`${apiDomain}/api/submit-destination-info`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: `{
          "Name" : "${name}",
          "MaximumWeight" : ${weight}
        }`
      })
      .then(response => response.json())
      .then(responseData => handleSubmitResponse(responseData))
    
    } catch (error) {
      handleError(error);
    }
  };

  const handleError = (error) => {
    console.error(error);
    resetFields();
    setLoading(false);
  }

  const getData = () => {
    fetch(`${apiDomain}/api/get-destination-info`)
      .then(response => response.json())
      .then(json => handleGetDataResponse(json))
      .catch(error => handleError(error));
  }

  const addIdToEachObjectInArray = (arr) => {
    let newArr = [];
    
    arr.forEach((item) => {
      newArr.push({...item, id: item.rowKey})
    })

    return newArr;
  }

  React.useEffect(() => {
    getData()
  }, []);

  return (
    <Box
      id="destination-setup-box"
      sx={(theme) => ({
        width: '100%',
        backgroundImage: 'linear-gradient(#225991, #fdfdff)',
        backgroundSize: '100% 125px',
        backgroundRepeat: 'no-repeat',
      })}
    >
      <Container
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          pt: { xs: 14, sm: 20 },
          pb: { xs: 8, sm: 12 },
        }}
      >
        <Stack 
          spacing={1} 
          useFlexGap sx={{ width: { xs: '100%', sm: '100%' } }}
          direction={{ xs: 'column', sm: 'column' }}
        >
          <Typography
            component="h2"
            variant="h2"
            sx={{
              display: 'flex',
              flexDirection: { xs: 'column', md: 'row' },
              alignSelf: 'center',
              textAlign: 'center',
            }}
          >
            Destination&nbsp;
            <Typography
              component="span"
              variant="h2"
              sx={{ color: 'primary.main' }}
            >
              Setup
            </Typography>
          </Typography>

          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            alignSelf="left"
            spacing={3}
            useFlexGap
            sx={{ pt: 1, width: { xs: '100%', sm: 'auto' } }}
          >
            {/* form */}
            <Stack
              direction={{ xs: 'column', sm: 'column' }}
              alignSelf="left"
              spacing={1}
              useFlexGap
              sx={{ pt: 4, width: { xs: '100%', sm: '20%' } }}
            >
              <TextField
                id="name"
                size="small"
                variant="outlined"
                aria-label="Enter Destination Name"
                placeholder="Destination Name"
                inputProps={{
                  autocomplete: 'off',
                  ariaLabel: 'Enter Requestor Name',
                }}
                sx={{ width: { xs: '100%', sm: '219px' } }}
                value={name}
                onInput={ e=>setName(e.target.value)}
              />
              <TextField
                id="weight"
                size="small"
                variant="outlined"
                aria-label="Enter Maximum Weight"
                placeholder="Maximum Weight"
                inputProps={{
                  autocomplete: 'off',
                  ariaLabel: 'Enter Maximum Weight',
                }}
                sx={{ width: { xs: '100%', sm: '219px' } }}
                value={weight}
                onInput={ e=>setWeight(e.target.value)}
              />
              <Button 
                sx={{ width: { xs: '100%', sm: '219px' } }} 
                variant="contained" 
                color="primary"
                disableRipple
                onClick={handleSubmit}
              >
                Submit
              </Button>
            </Stack>

            {/* datagrid */}
            <Stack
              direction={{ xs: 'column', sm: 'column' }}
              alignSelf="right"
              spacing={1}
              useFlexGap
              sx={{ pt: 4, width: { xs: '100%', sm: '80%' } }}
            >
              {loading && <LinearProgress /> }
              {!loading && 
                <DataGrid 
                  rows={destinationInfo} 
                  columns={gridColumns} 
                  pageSizeOptions={[10, 25, 50]}
                  initialState={{
                    pagination: { paginationModel: { pageSize: 10 } }
                  }}
                />
              }
            </Stack>

          </Stack>
        </Stack>
      </Container>
    </Box>
  );
}