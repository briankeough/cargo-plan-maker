import * as React from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import LinearProgress from '@mui/material/LinearProgress';
import Modal from '@mui/material/Modal';
import ErrorIcon from '@mui/icons-material/Error'
import { DataGrid } from '@mui/x-data-grid';
import { apiDomain } from '../config/Config';

export default function ItemSpecifications() {

  const gridColumns = [
    { field: 'nsn', headerName: 'NSN', flex: 1, resizable: true },
    { field: 'name', headerName: 'Name', flex: 1, resizable: true },
    { field: 'weight', headerName: 'Weight', flex: 1, resizable: true },
    { field: 'length', headerName: 'Length', flex: 1, resizable: true },
    { field: 'width', headerName: 'Width', flex: 1, resizable: true },
    { field: 'height', headerName: 'Height', flex: 1, resizable: true },
    { field: 'cannotFlipOnSide', headerName: 'Cannot Flip', flex: 1, resizable: true },
  ];
  
  const [itemSpecData, setItemSpecData] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [openModal, setOpenModal] = React.useState(false);
  const [selectedFile, setSelectedFile] = React.useState();

  const [incompleteItems, setIncompleteItems] = React.useState([]);
  const [nigoFormatItems, setNigoFormatItems] = React.useState([]);
  const [duplicateNsns, setDuplicateNsns] = React.useState([]);
  
  const resetFields = () => {
    setLoading(false);
  }

  const modalStyle = {
    position: 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    bgcolor: 'background.paper',
    boxShadow: 24,
    p: 6,
  };
  
  const handleModalClose = () => setOpenModal(false);

  const incompleteItemsList = incompleteItems.map((item) =>
    <li>{item}</li>
  );

  const nigoFormatItemsList = nigoFormatItems.map((item) =>
    <li>{item}</li>
  );

  const duplicateNsnsList = duplicateNsns.map((item) =>
    <li>{item}</li>
  );

  
  const handleGetDataResponse = (json) => {
    setItemSpecData(addIdToEachObjectInArray(json));
    setLoading(false);
  }

  const handleSubmitResponse = (json) => {
    let status = json.status;

    if (status === 'success'){
      console.log(`Successfully uploaded item specs`)
      setLoading(false);
      setSelectedFile(null);
      getData();
 
    } else {
      setLoading(false);
      console.log(`Not successfully saved. Status: ${status}`)

      setIncompleteItems(json.incompleteItems);
      setNigoFormatItems(json.nigoFormatItems);
      setDuplicateNsns(json.duplicateNsns);

      setOpenModal(true);
    }
  }

  const handleSubmit = async () => {
    setLoading(true);

    const formData = new FormData();
    formData.append("file", selectedFile);

    try {
      await fetch(`${apiDomain}/api/submit-item-specs`, {
        method: "POST",
        body: formData,
      })
      .then(response => response.json())
      .then(responseData => handleSubmitResponse(responseData))
    
    } catch (error) {
      handleError(error);
    }
  };

  const handleFileUpload = ({ target }) => {
    setSelectedFile(target.files[0]);
  }

  const handleError = (error) => {
    console.error(error);
    resetFields();
    setLoading(false);
  }

  const getData = () => {
    fetch(`${apiDomain}/api/get-all-item-specs`)
      .then(response => response.json())
      .then(json => handleGetDataResponse(json))
      .catch(error => console.error(error));
  }

  const addIdToEachObjectInArray = (arr) => {
    let newArr = [];
    
    arr.forEach((item) => {
      newArr.push({
        ...item, 
        id: item.rowKey, 
        weight: Number(item.weight), 
        length: Number(item.length), 
        width: Number(item.width), 
        height: Number(item.height)})
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
            Item&nbsp;
            <Typography
              component="span"
              variant="h2"
              sx={{ color: 'primary.main' }}
            >
              Specifications
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
              <Button 
                sx={{ width: { xs: '100%', sm: '219px' } }} 
                variant="outlined" 
                component="label"
              >
                Upload Item Specs
                <input 
                  type="file" 
                  hidden 
                  onChange={handleFileUpload}
                />
              </Button>
              
              {selectedFile && <div>{selectedFile.name}</div>}

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
                  rows={itemSpecData} 
                  columns={gridColumns} 
                  pageSizeOptions={[10, 25, 50]}
                  initialState={{
                    pagination: { paginationModel: { pageSize: 25 } }
                  }}
                />
              }

              <Modal
                open={openModal}
                onClose={handleModalClose}
                aria-labelledby="modal-modal-title"
                aria-describedby="modal-modal-description"
              >
                <Box sx={modalStyle}>
                  <Typography id="modal-modal-title" variant="h4" component="h2">
                    <ErrorIcon  sx={{ fill: '#1976d2', margin: '0px 8px 0px 0px', verticalAlign: 'middle', fontSize: '2em' }} />
                    Problems Found
                  </Typography>
                  {incompleteItems && incompleteItems.length > 0 &&
                    <div>
                      <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        Items With Incomplete Data:
                      </Typography>
                      <ul>{incompleteItemsList}</ul>
                    </div>
                  }
                  {nigoFormatItems && nigoFormatItems.length > 0 &&
                    <div>
                      <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        Items With Incorrectly Formatted Data:
                      </Typography>
                      <ul>{nigoFormatItemsList}</ul>
                    </div>
                  }
                  {duplicateNsns && duplicateNsns.length > 0 && 
                    <div>
                      <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        Duplicate NSNs With Different Data:
                      </Typography>
                      <ul>{duplicateNsnsList}</ul>
                    </div>
                  }
                </Box>
              </Modal>
              
            </Stack>
          </Stack>
        </Stack>
      </Container>
    </Box>
  );
}